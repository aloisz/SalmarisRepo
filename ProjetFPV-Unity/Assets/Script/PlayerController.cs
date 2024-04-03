using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : GenericSingletonClass<PlayerController>
    {
        [Header("Overall Behavior")]
        public bool canMove = true;
        
        //---------------------------------------
        
        [Header("Components")]
        [SerializeField] internal PlayerScriptable playerScriptable;
        [SerializeField] private Transform cameraAttachPosition;
        [SerializeField] private Collider capsuleCollider;
        
        [HideInInspector] public Rigidbody _rb;
        
        //---------------------------------------

        [Header("Values")]
        internal Vector3 direction;
        internal Vector3 directionNotReset;

        private float moveSpeed;
        private float _velocity;
        private float _rotationX;
        
        private float dashTimer;
        private float idleTimer;

        private float actualSlopeAngle;
        
        private const float _gravity = -9.81f;
        
        //---------------------------------------

        [Header("Momentum")] 
        private float dashTimerSpeedAdd;
        private float speedMultiplierFromDash = 1f;
        
        //---------------------------------------
        
        [Header("States")]
        public bool isOnGround;
        public bool wasOnGroundLastFrame;
        public bool isMoving;
        public bool isSliding;
        public bool isJumping;
        public bool isDashing;
        public bool isOnSlope;
        public bool isSlopeClimbing;

        internal PlayerActionStates currentActionState;
        
        private bool canJump;
        private bool canDash;
        private bool canApplyGravity = true;
        
        private bool isAccelerating;
        private bool isDecelerating;

        //---------------------------------------

        [Header("Detection")] 
        [SerializeField] private LayerMask groundLayer;

        private RaycastHit raycastGroundRight;
        private RaycastHit raycastGroundLeft;
        private RaycastHit raycastGroundForward;
        private RaycastHit raycastGroundBack;
        
        private RaycastHit raycastSlope;
        private RaycastHit raycastSlopeFront;
        
        internal enum PlayerActionStates
        {
            Idle,
            Moving,
            Sliding,
            Jumping,
            Dashing,
        }
        
        //---------------------------------------

        public override void Awake()
        {
            base.Awake();
            
            //get the rigidbody component.
            _rb = GetComponent<Rigidbody>();
            
            Cursor.lockState = CursorLockMode.Locked;
        }
        private void Update()
        {
            PlayerStateMachine();

            ManageDashDuration();
            ManageSpeedMultiplierFromDash();
            
            SetMoveSpeed();

            RechargeStamina();
            
            DetectIdling();
            
            DetectSlope();
            
            DetectGround();
            
            //CoyoteJump();

            RotateCameraFromInput();

            //Debug
            if (Input.GetKeyDown(KeyCode.Keypad1)) Time.timeScale = 0.1f;
            if (Input.GetKeyDown(KeyCode.Keypad2)) Time.timeScale = 0.5f;
            if (Input.GetKeyDown(KeyCode.Keypad3)) Time.timeScale = 1f;
        }
        
        private void FixedUpdate()
        {
            Move();
            ManageGravity();
            
            SetDrag();
            
            PlayerInputs.Instance.onJump?.Invoke();
            VerifyDashExecution();
        }

        //-------------------- Movements ----------------------

        #region Movements

        private float _decelerationSlideOnGround;
        
        /// <summary>
        /// Move the player in the current direction value, based on scriptable parameters.
        /// </summary>
        private void Move()
        {
            //Set the current input values to the direction.
            var dirFromInputs = PlayerInputs.Instance.moveValue;
            direction = new Vector3(dirFromInputs.x, 0, dirFromInputs.y).normalized;
            
            //If the direction isn't null, set the direction not reset to direction.
            if (direction.magnitude > playerScriptable.moveThreshold) directionNotReset = direction;
            
            _rb.AddForce(GetOverallMomentumVector(), ForceMode.Impulse);

            if (_rb.velocity.magnitude > playerScriptable.maxRigidbodyVelocity) 
                _rb.velocity = Vector3.ClampMagnitude(_rb.velocity, playerScriptable.maxRigidbodyVelocity);
        }
        
        
        private Vector3 GetOverallMomentumVector()
        {
            var dividerOnSlopeClimbing = (isSlopeClimbing && isSliding ? playerScriptable.decelerationMultiplierSlideInSlopeUp : 1f);
            var accelerating = _rb.velocity.magnitude < playerScriptable.speedMaxToAccelerate && !isOnSlope && !isSliding;
            var vectorMove = DirectionFromCamera(direction).normalized * 
                             (moveSpeed * speedMultiplierFromDash * (accelerating ? playerScriptable.accelerationMultiplier : 1f))
                             / dividerOnSlopeClimbing;
            
            var slopeDirection = new Vector3(raycastSlope.normal.x, 0, raycastSlope.normal.z).normalized;
            var vectorSlideDown = Vector3.down * (playerScriptable.slidingInSlopeDownForce) / dividerOnSlopeClimbing;
            var vectorSlideForward = (slopeDirection * (actualSlopeAngle / playerScriptable.slidingInSlopeLimiter)) / dividerOnSlopeClimbing;
            var vectorSlide = vectorSlideDown + vectorSlideForward;
            
            if(isSliding && !isOnSlope) _decelerationSlideOnGround += Time.deltaTime * playerScriptable.decelerationMultiplierSlideOnGround;
            
            var finalVector = ((isMoving ? vectorMove : Vector3.zero) + 
                               (isOnSlope && isSliding && !isSlopeClimbing ? vectorSlide : Vector3.zero))
                              / (isSliding && !isOnSlope ? _decelerationSlideOnGround : 1f);
            
            return finalVector / (isSliding && isMoving && isOnSlope ? playerScriptable.overallMomentumLimiterMoveSlideInSlope : 1f);
        }
        
        private void SetMoveSpeed()
        {
            moveSpeed = isOnGround ? playerScriptable.moveSpeed : 
                playerScriptable.moveSpeed / playerScriptable.moveSpeedInAirDivider;
        }
        
        private void ManageDashDuration()
        {
            dashTimer.DecreaseTimerIfPositive();
            if (dashTimer <= 0f)
            {
                canApplyGravity = true;
                _rb.useGravity = true;
                isDashing = false;
            }
        }
        
        private void ManageSpeedMultiplierFromDash()
        {
            //Decrease the duration while the value is positive.
            dashTimerSpeedAdd.DecreaseTimerIfPositive();
            
            //Set the speed multiplier from dash to the added % value if the timer isn't finished.
            // Lerp the speed to the multiplier in 1s.
            // Lerp the speed to the basic value in Xs.
            speedMultiplierFromDash = dashTimerSpeedAdd > 0 ? 
                Mathf.Lerp(speedMultiplierFromDash, playerScriptable.dashSpeedMultiplier, Time.deltaTime) : 
                Mathf.Lerp(speedMultiplierFromDash, 1f, Time.deltaTime / playerScriptable.dashSpeedMultiplierResetDuration);
        }
        
        /// <summary>
        /// Recharge the player's stamina when he is on the ground or less if he's in the air.
        /// </summary>
        private void RechargeStamina()
        {
            if(!isDashing && !isJumping)
                PlayerStamina.Instance.GenerateStaminaStep(playerScriptable.staminaPerSecond);
            
            else if(isJumping)
                //Generate two times less stamina when in this airs.
                PlayerStamina.Instance.GenerateStaminaStep(playerScriptable.staminaPerSecond / 2f);
        }
        
        /// <summary>
        /// Manage the coyote jump and timer, and reset it.
        /// </summary>
        private void CoyoteJump()
        {
            //if (!isOnGround) coyoteTimer.DecreaseTimerIfPositive();
            //else coyoteTimer = playerScriptable.coyoteJump;
        }

        #endregion
        
        //-------------------- Camera ----------------------
        
        #region Camera
        public Vector3 DirectionFromCamera(Vector3 dir)
        {
            Vector3 camForward = cameraAttachPosition.forward;
            Vector3 camRight = cameraAttachPosition.right;

            camForward.y = 0;
            camRight.y = 0;

            Vector3 forwardRelative = dir.z * camForward;
            Vector3 rightRelative = dir.x * camRight;

            return forwardRelative + rightRelative;
        }
        
        #endregion
        
        //-------------------- Physic ----------------------
        
        #region Physic
        
        /// <summary>
        /// Managing the gravity by applying a force on the Y velocity axis, and adapt the XZ from the camera direction.
        /// </summary>
        private void ManageGravity()
        {
            if (canApplyGravity)
            {
                var velocity = _rb.velocity;
                
                var v = velocity;
                
                v.y -= Time.deltaTime * playerScriptable.gravityMultiplier;
                
                v.x = velocity.x + (DirectionFromCamera(direction).x * playerScriptable.moveAirMultiplier);
                v.z = velocity.z + (DirectionFromCamera(direction).z * playerScriptable.moveAirMultiplier);
                
                _rb.velocity = v;
            }
        }

        /// <summary>
        /// Detect the slope under the player, check if the player is climbing the slope or falling onto it.
        /// </summary>
        private void DetectSlope()
        {
            Physics.Raycast(transform.position + new Vector3(0,0.35f,0), Vector3.down, out raycastSlope, playerScriptable.raycastLenghtSlopeDetection,
                groundLayer);
            
            Physics.Raycast(transform.position + new Vector3(0,0.35f,0) + (transform.forward * 2f), Vector3.down, 
                out raycastSlopeFront, playerScriptable.raycastLenghtSlopeDetection, groundLayer);

            actualSlopeAngle = Vector3.Angle(raycastSlope.normal, Vector3.up);

            if (actualSlopeAngle > playerScriptable.minSlopeDegrees) 
            {
                isOnSlope = true;
                isSlopeClimbing = raycastSlopeFront.point.y > transform.position.y;
            }
            else
            {
                isOnSlope = false;
                isSlopeClimbing = false;
            }
        }
        
        /// <summary>
        /// Set the physical material of the player, from it's different states.
        /// </summary>
        /// <param name="s">The state to set the material with.</param>
        private void SetPhysicalMaterialCollider(PlayerActionStates s)
        {
            var chosenMat = s == PlayerActionStates.Idle
                ? playerScriptable.frictionMaterial
                : playerScriptable.movingMaterial;
            
            //if the capsule hasn't the material yet, apply it.
            if (capsuleCollider.sharedMaterial != chosenMat)
            {
                capsuleCollider.material = chosenMat;
            }
        }
        
        /// <summary>
        /// Set the rigidbody drag of the player, from it's different states.
        /// </summary>
        private void SetDrag()
        {
            if (isOnGround && isSliding && isOnSlope && !isSlopeClimbing)
            {
                _rb.drag = 0f;
            }
            else if (isOnGround && isSliding && !isOnSlope && !isSlopeClimbing)
            {
                _rb.drag = 0.65f;
                if (isOnGround && isSliding && !isOnSlope && _rb.velocity.magnitude < 25f)
                {
                    _rb.drag = 10f;
                }
            }
            else if (isOnGround && isSliding && isOnSlope && isSlopeClimbing && _rb.velocity.magnitude < 25f)
            {
                _rb.drag = 5f;
            }
            else if (isOnSlope && !isSliding)
            {
                _rb.drag = playerScriptable.groundDrag;
            }
            else if (isOnGround && isSliding && !isOnSlope && _rb.velocity.magnitude < 20f)
            {
                _rb.drag = 10f;
            }
            else if (isOnGround && !isSliding)
            {
                _rb.drag = playerScriptable.groundDrag;
            }
            else
            {
                _rb.drag = playerScriptable.airDrag;
            }
        }
        
        #endregion
        
        //-------------------- Detections ----------------------
        
        #region Detections
        
        /// <summary>
        /// Detect the ground by drawing an overlapBoxNonAlloc, and manage the landing verification.
        /// </summary>
        void DetectGround()
        {
            var offset = playerScriptable.groundDetectionForwardOffset;
            var pos = transform.position + new Vector3(0,playerScriptable.groundDetectionUpOffset,0);
            
            var posCheckRight = ReturnCheckOffsetFromDir(pos, Helper.ReturnDirFromIndex(0), offset);
            var isOnGroundTempRight = Physics.Raycast(posCheckRight, Vector3.down * playerScriptable.groundDetectionLenght, out raycastGroundRight, 
                playerScriptable.groundDetectionLenght, groundLayer);
            
            var posCheckLeft = ReturnCheckOffsetFromDir(pos, Helper.ReturnDirFromIndex(1), offset);
            var isOnGroundTempLeft = Physics.Raycast(posCheckLeft, Vector3.down * playerScriptable.groundDetectionLenght, out raycastGroundLeft, 
                playerScriptable.groundDetectionLenght, groundLayer);
            
            var posCheckForward = ReturnCheckOffsetFromDir(pos, Helper.ReturnDirFromIndex(2), offset);
            var isOnGroundTempForward = Physics.Raycast(posCheckForward, Vector3.down * playerScriptable.groundDetectionLenght, out raycastGroundForward, 
                playerScriptable.groundDetectionLenght, groundLayer);
            
            var posCheckBack = ReturnCheckOffsetFromDir(pos, Helper.ReturnDirFromIndex(3), offset);
            var isOnGroundTempBack = Physics.Raycast(posCheckBack, Vector3.down * playerScriptable.groundDetectionLenght, out raycastGroundBack, 
                playerScriptable.groundDetectionLenght, groundLayer);

            if (isOnGroundTempRight || isOnGroundTempLeft || isOnGroundTempForward || isOnGroundTempBack)
            {
                if (!isOnGround)
                {
                    isOnGround = true;
                    StartCoroutine(nameof(OnLand));
                }
            }
            else
            {
                isOnGround = false;
            }
            
            //Update the last frame ground state.
            wasOnGroundLastFrame = isOnGround;
        }

        /// <summary>
        /// Return a position from an offset and a lenght of offset. Used for ground checking.
        /// </summary>
        /// <param name="pos">The base position where the offset will be applied.</param>
        /// <param name="dir">The direction where appling the offset.</param>
        /// <param name="offset">The amount of offset to apply.</param>
        /// <returns></returns>
        private Vector3 ReturnCheckOffsetFromDir(Vector3 pos, Vector3 dir, float offset) => pos + dir * offset;
        
        /// <summary>
        /// On land, execute and reset variables.
        /// </summary>
        IEnumerator OnLand()
        {
            canApplyGravity = false;
            yield return new WaitForSeconds(0.025f);
            isJumping = false;
        }

        #endregion
        
        //-------------------- Inputs ----------------------
        
        #region Inputs

        /// <summary>
        /// Make the player jump, at different heights if he's in coyote jump or not. 
        /// </summary>
        private void Jump()
        {
            isJumping = true;
            
            var forwardMomentumVector = GetOverallMomentumVector() / 20f;
            _rb.AddForce(playerScriptable.jumpForce * (Vector3.up + new Vector3(forwardMomentumVector.x, 0, forwardMomentumVector.z)), ForceMode.Impulse);
            
            //coyoteTimer = 0f;
        }

        /// <summary>
        /// Make the camera rotate from where the player look.
        /// </summary>
        private void RotateCameraFromInput()
        {
            var rot = PlayerInputs.Instance.rotateValue;
            
            _rotationX += -rot.y * playerScriptable.sensibility;
            _rotationX = Mathf.Clamp(_rotationX, -playerScriptable.lookLimitY, playerScriptable.lookLimitY);
            cameraAttachPosition.localRotation = Quaternion.Euler(_rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, rot.x * playerScriptable.sensibility, 0);
        }

        private void Dash()
        {
            var dashDirectionConvert = Helper.ConvertTo4Dir(new Vector2(direction.x, direction.z));
            var dirFromCam = new Vector3(Mathf.RoundToInt(dashDirectionConvert.x), 0, Mathf.RoundToInt(dashDirectionConvert.y));
            var dashDirection = DirectionFromCamera(dirFromCam);
            var dashDirectionNoY = new Vector3(dashDirection.x, 0, dashDirection.z);

            transform.position += new Vector3(0, 0.5f, 0);
            
            _rb.AddForce((dashDirectionNoY.magnitude < 0.1f ? transform.forward : dashDirectionNoY) * 
                         playerScriptable.dashForce, ForceMode.Impulse);

            canApplyGravity = false;
            _rb.useGravity = false;
            
            dashTimer = playerScriptable.dashDuration;
            dashTimerSpeedAdd = playerScriptable.dashSpeedMultiplierDuration;

            isDashing = true;

            _rb.velocity = Vector3.zero;
        }

        private void VerifyDashExecution()
        {
            if (canDash)
            {
                PlayerStamina.Instance.ConsumeStaminaStep(1);
                Dash();
            }
        }

        public void VerifyJumpExecution()
        {
            if(canJump) Jump();
            PlayerInputs.Instance.onJump -= VerifyJumpExecution;
        }
        
        #endregion
        
        //-------------------- State Machine ----------------------

        #region StateMachine
        void PlayerStateMachine()
        {
            canDash = PlayerInputs.Instance.isReceivingDashInputs && !isDashing && PlayerStamina.Instance.HasEnoughStamina(1);
            canJump = isOnGround && !isJumping;
            
            isSliding = PlayerInputs.Instance.isReceivingSlideInputs && isOnGround;
            isMoving = direction.magnitude > playerScriptable.moveThreshold;
            
            if (isMoving && !isSliding && !isJumping && !isDashing) currentActionState = PlayerActionStates.Moving;
            
            else if(isSliding && !isJumping && !isDashing) currentActionState = PlayerActionStates.Sliding;
            
            else if (!isSliding && isJumping && !isDashing) currentActionState = PlayerActionStates.Jumping;
                
            else if(isDashing) currentActionState = PlayerActionStates.Dashing;

            else if (idleTimer <= 0f) currentActionState = PlayerActionStates.Idle;

            else currentActionState = PlayerActionStates.Moving;

            switch (currentActionState)
            {
                case PlayerActionStates.Idle: OnIdle();
                    break;
                case PlayerActionStates.Moving: _decelerationSlideOnGround = 1f;
                    break;
                case PlayerActionStates.Sliding:
                    break;
                case PlayerActionStates.Jumping:
                    break;
                case PlayerActionStates.Dashing:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            SetPhysicalMaterialCollider(currentActionState);
        }

        private void DetectIdling()
        {
            if (isMoving && isOnGround) idleTimer = playerScriptable.timeBeforeDetectedIdle;
            else idleTimer.DecreaseTimerIfPositive();
        }

        private void OnIdle()
        {
            //Reset the current dash duration. Make this cancel the dash.
            dashTimer = 0f;
            
            //Reset the timer of the speed dash bonus.
            dashTimerSpeedAdd = 0f;
            
            //Reset the speed bonus from dash.
            speedMultiplierFromDash = 1f;
            
            //Reset the deceleration for the slide on ground.
            _decelerationSlideOnGround = 1f;
        }
        
        #endregion

        //-------------------- Debug ----------------------

        #region Debug
        private void OnDrawGizmos()
        {
            var position = transform.position;

            //Slope Detection
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(position + new Vector3(0, 0.35f, 0),
                Vector3.down * playerScriptable.raycastLenghtSlopeDetection);
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position + new Vector3(0,0.35f,0) + (transform.forward * 2f),
                Vector3.down * playerScriptable.raycastLenghtSlopeDetection);

            //Slope Normal
            Gizmos.color = Color.green;
            Gizmos.DrawRay(raycastSlope.point, raycastSlope.normal * 2.5f);
            
            var offset = playerScriptable.groundDetectionForwardOffset;
            var pos = transform.position + new Vector3(0,playerScriptable.groundDetectionUpOffset,0);

            for (int i = 0; i < 4; i++)
            {
                var posCheck = ReturnCheckOffsetFromDir(pos, Helper.ReturnDirFromIndex(i), offset);
                
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(posCheck, Vector3.down * playerScriptable.groundDetectionLenght);
            }
        }

        private void OnGUI()
        {
            // Set up GUI style for the text
            GUIStyle style = new GUIStyle
            {
                fontSize = 24,
                normal =
                {
                    textColor = Color.white
                }
            };

            // Set the position and size of the text
            // 60 each part
            // 30 each elements
            Rect rect = new Rect(10, 10, 200, 50);
            Rect rect1 = new Rect(10, 40, 200, 50);
            
            Rect rect2 = new Rect(10, 100, 200, 50);
            Rect rect3 = new Rect(10, 130, 200, 50);
            
            Rect rect4 = new Rect(10, 190, 200, 50);
            
            Rect rect5 = new Rect(10, 250, 200, 50);
            
            Rect rect6 = new Rect(10, 310, 200, 50);
            
            Rect rect7 = new Rect(10, 370, 200, 50);
            
            Rect rect8 = new Rect(10, 430, 200, 50);
            Rect rect9 = new Rect(10, 460, 200, 50);
            
            Rect rect10 = new Rect(10, 520, 200, 50);
            
            Rect rect11 = new Rect(10, 160, Mathf.Lerp(1,300,_rb.velocity.magnitude / 100f), 10);

            // Display the text on the screen
            GUI.Label(rect, $"Direction : {direction}", style);
            GUI.Label(rect1, $"Direction No reset : {directionNotReset}", style);
            
            GUI.Label(rect2, $"Rigidbody Velocity : {_rb.velocity}", style);
            GUI.Label(rect3, $"Rigidbody Magnitude : {_rb.velocity.magnitude}", style);
            GUI.DrawTexture(rect11, new Texture2D((int)Mathf.Lerp(1,300,_rb.velocity.magnitude / 100f), 10),
                ScaleMode.ScaleToFit, true, 0, Color.Lerp(Color.green, Color.red, _rb.velocity.magnitude / 100f), 
                0, 0);
            
            GUI.Label(rect4, $"Current State : {Convert.ToString(currentActionState)}", style);
            
            GUI.Label(rect5, $"Overall Momentum Vector : {GetOverallMomentumVector()}", style);
            
            GUI.Label(rect6, $"Grounded ? : {isOnGround}", BoolStyle(isOnGround));
            
            GUI.Label(rect7, $"Is On Slope ? : {isOnSlope}", BoolStyle(isOnSlope));

            var text = raycastSlope.collider ? new Vector3(raycastSlope.normal.x, 0, raycastSlope.normal.z).normalized : Vector3.zero;
            GUI.Label(rect8, $"Current Slope Direction : {text}", style);
            GUI.Label(rect9, $"Current Slope Angle : {actualSlopeAngle}", style);
            
            GUI.Label(rect10, $"Rigidbody Drag : {_rb.drag}", style);
        }

        GUIStyle BoolStyle(bool value)
        {
            var style = new GUIStyle
            {
                fontSize = 24,
                normal =
                {
                    textColor = value ? Color.green : Color.red
                }
            };
            return style;
        }
        
        #endregion
        
        //-------------------- Unused ----------------------
        
        #region Unused
        
        #endregion
    }
}

