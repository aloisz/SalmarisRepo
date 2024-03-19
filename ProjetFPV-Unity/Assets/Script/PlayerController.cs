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
        
        private Rigidbody _rb;
        
        //---------------------------------------

        [Header("Values")]
        internal Vector3 direction;
        internal Vector3 directionNotReset;

        private float moveSpeed;
        private float _velocity;
        private float _rotationX;
        
        private float dashTimer;
        private float idleTimer;
        private float coyoteTimer;

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
            PlayerInputStateMachine();
            
            DetectGround();

            ManageDashDuration();
            ManageSpeedMultiplierFromDash();
            
            SetMoveSpeed();

            RechargeStamina();
            
            DetectIdling();
            
            DetectSlope();
            
            CoyoteJump();

            RotateCameraFromInput();

            if (Input.GetKeyDown(KeyCode.Keypad1)) Time.timeScale = 0.1f;
            if (Input.GetKeyDown(KeyCode.Keypad2)) Time.timeScale = 0.5f;
            if (Input.GetKeyDown(KeyCode.Keypad3)) Time.timeScale = 1f;

            canDash = PlayerInputs.Instance.isReceivingDashInputs && !isDashing && PlayerStamina.Instance.HasEnoughStamina(1);
            canJump = (coyoteTimer > 0f && PlayerInputs.Instance.isReceivingJumpInputs) || (isOnGround && PlayerInputs.Instance.isReceivingJumpInputs);
            isSliding = PlayerInputs.Instance.isReceivingSlideInputs /*&& isMoving*/ && isOnGround;
        }
        
        private void FixedUpdate()
        {
            Move();
            ManageGravity();
            
            SetDrag();
            
            VerifyJumpExecution();
            VerifyDashExecution();
        }

        //-------------------- Movements ----------------------

        #region Movements
        
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
            
            //Setup the basic direction
            var dir = DirectionFromCamera(direction).normalized * GetOverallSpeed();
            
            //Slope interaction
            var slopeDirection = new Vector3(raycastSlope.normal.x, 0, raycastSlope.normal.z).normalized;
            
            if (isOnSlope && isSliding)
            {
                _rb.AddForce(Vector3.down * (200f * Time.deltaTime), ForceMode.Impulse);
                _rb.AddForce(slopeDirection * (200f * Time.deltaTime), ForceMode.Impulse);
            }

            //Basic movement managing
            if (_rb.velocity.magnitude < playerScriptable.speedMaxToAccelerate)
            {
                _rb.AddForce((GetOverallSpeed() * playerScriptable.accelerationMultiplier) * dir, ForceMode.Impulse);
            }
            else
            {
                _rb.AddForce(GetOverallSpeed() * dir, ForceMode.Impulse);
            }
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

        private float GetOverallSpeed()
        {
            return (moveSpeed * speedMultiplierFromDash) * (isSliding && !isOnSlope ? _rb.velocity.magnitude / 200f : 1f);
        }

        
        /// <summary>
        /// Manage the coyote jump and timer, and reset it.
        /// </summary>
        private void CoyoteJump()
        {
            if (!isOnGround) coyoteTimer.DecreaseTimerIfPositive();
            else coyoteTimer = playerScriptable.coyoteJump;
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
                v.z = _rb.velocity.z + (DirectionFromCamera(direction).z * playerScriptable.moveAirMultiplier);
                
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
            _rb.drag = isOnGround ? playerScriptable.groundDrag : playerScriptable.airDrag;
            if (!isOnSlope && isOnGround && !isMoving) _rb.drag = 0f;
        }
        
        #endregion
        
        //-------------------- Detections ----------------------
        
        #region Detections
        
        /// <summary>
        /// Detect the ground by drawing an overlapBoxNonAlloc, and manage the landing verification.
        /// </summary>
        void DetectGround()
        {
            var transform1 = transform;
            var position = transform1.position;
            var rotation = transform1.rotation;
            var forward = transform1.forward;
            
            //Security array to stack ground colliders detected, fixed to 10 max colliders.
            Collider[] hit = new Collider[10];
            
            //Create an offset for the boxcast.
            var offset = forward * playerScriptable.groundDetectionForwardOffset;
            
            //Check if the player is on the ground or not, by an overlapBoxNonAlloc.
            isOnGround =
                Physics.OverlapBoxNonAlloc(position - offset,
                    playerScriptable.groundDetectionWidthHeightDepth, hit, rotation, groundLayer) > 0;

            //Debug the overlapBoxNonAlloc.
            ExtDebug.DrawBoxCastBox(position - offset,
                playerScriptable.groundDetectionWidthHeightDepth, rotation, Vector3.zero, 0.2f, Color.cyan);

            //Check if the player just landed.
            if (isOnGround && !wasOnGroundLastFrame)
            {
                OnLand();
            }

            //Update the last frame ground state.
            wasOnGroundLastFrame = isOnGround;
        }

        /// <summary>
        /// On land, execute and reset variables.
        /// </summary>
        private void OnLand()
        {
            isJumping = false;
            canApplyGravity = false;
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

            var forwardMomentumVector = transform.forward * (new Vector3(_rb.velocity.x, 0, _rb.velocity.z).magnitude / 50f);
            
            if(coyoteTimer < playerScriptable.coyoteJump - 0.1f)
                _rb.AddForce(playerScriptable.coyoteJumpForce * (Vector3.up + forwardMomentumVector), ForceMode.Impulse);
            else
                _rb.AddForce(playerScriptable.jumpForce * (Vector3.up + forwardMomentumVector), ForceMode.Impulse);
            
            coyoteTimer = 0f;
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
            var dirFromCam = new Vector3(dashDirectionConvert.x, 0, dashDirectionConvert.y);
            var dashDirection = DirectionFromCamera(dirFromCam);
                
            _rb.AddForce(dashDirection * playerScriptable.dashForce, ForceMode.Impulse);

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

        private void VerifyJumpExecution()
        {
            if(canJump) Jump();
        }
        
        #endregion
        
        //-------------------- State Machine ----------------------

        #region StateMachine
        void PlayerInputStateMachine()
        {
            isMoving = direction.magnitude > playerScriptable.moveThreshold;

            if (isMoving && !isSliding && !isJumping && !isDashing) currentActionState = PlayerActionStates.Moving;
            
            else if(isSliding && !isJumping && !isDashing) currentActionState = PlayerActionStates.Sliding;
            
            else if (!isSliding && isJumping && !isDashing) currentActionState = PlayerActionStates.Jumping;
                
            else if(isMoving && isDashing) currentActionState = PlayerActionStates.Dashing;

            else if (idleTimer <= 0f) currentActionState = PlayerActionStates.Idle;

            else currentActionState = PlayerActionStates.Moving;

            switch (currentActionState)
            {
                case PlayerActionStates.Idle: OnIdle();
                    break;
                case PlayerActionStates.Moving:
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
            // 70 each part
            // 50 each elements
            Rect rect = new Rect(10, 10, 200, 50);
            Rect rect1 = new Rect(10, 60, 200, 50);
            
            Rect rect2 = new Rect(10, 130, 200, 50);
            Rect rect3 = new Rect(10, 180, 200, 50);
            
            Rect rect4 = new Rect(10, 250, 200, 50);
            
            Rect rect5 = new Rect(10, 320, 200, 50);
            
            Rect rect6 = new Rect(10, 390, 200, 50);
            
            Rect rect7 = new Rect(10, 460, 200, 50);
            
            Rect rect8 = new Rect(10, 530, 200, 50);

            // Display the text on the screen
            GUI.Label(rect, $"Direction : {direction}", style);
            GUI.Label(rect1, $"Direction No reset : {directionNotReset}", style);
            
            GUI.Label(rect2, $"Rigidbody Velocity : {_rb.velocity}", style);
            GUI.Label(rect3, $"Rigidbody Magnitude : {_rb.velocity.magnitude}", style);
            
            GUI.Label(rect4, $"Current State : {Convert.ToString(currentActionState)}", style);
            
            GUI.Label(rect5, $"Overall Speed : {GetOverallSpeed()}", style);
            
            GUI.Label(rect6, $"Grounded ? : {isOnGround}", BoolStyle(isOnGround));
            
            GUI.Label(rect7, $"Is On Slope ? : {isOnSlope}", BoolStyle(isOnSlope));

            var text = raycastSlope.collider ? new Vector3(raycastSlope.normal.x, 0, raycastSlope.normal.z).normalized : Vector3.zero;
            GUI.Label(rect8, $"Current Slope Direction : {text}", style);
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

