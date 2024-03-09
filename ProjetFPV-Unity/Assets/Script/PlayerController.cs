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
        public bool hasAnEdgeForward;
        
        internal PlayerActionStates currentActionState;
        
        private bool canJump;
        private bool canDash;
        private bool canApplyGravity = true;
        
        private bool isAccelerating;
        private bool isDecelerating;
        
        private bool receivedJumpInput;
        private bool receivedDashInput;
        
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
            DetectEdges();
            
            ManageDashDuration();
            ManageSpeedMultiplierFromDash();
            
            SetMoveSpeed();

            RechargeStaminaFromSpeed();
            
            DetectIdling();
            
            DetectSlope();
            
            CoyoteJump();

            if (Input.GetKeyDown(KeyCode.Keypad1)) Time.timeScale = 0.1f;
            if (Input.GetKeyDown(KeyCode.Keypad2)) Time.timeScale = 0.5f;
            if (Input.GetKeyDown(KeyCode.Keypad3)) Time.timeScale = 1f;

            canDash = receivedDashInput && !isDashing && PlayerStamina.Instance.HasEnoughStamina(1);
            canJump = (coyoteTimer > 0f && receivedJumpInput) || (isOnGround && receivedJumpInput);
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
            var dir = DirectionFromCamera(direction).normalized * GetOverallSpeed();
            
            #region unused
            /*var targetVelocity = new Vector3(dir.x, _rb.velocity.y, dir.z);
            _rb.velocity = Vector3.MoveTowards(_rb.velocity, targetVelocity * intertiaMultiplier, 
                Time.deltaTime * playerScriptable.accelerationSpeed);*/
            #endregion
            
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
            moveSpeed = isOnGround ? playerScriptable.moveSpeed : playerScriptable.moveSpeed / 2.5f;
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
        
        private void RechargeStaminaFromSpeed()
        {
            if(!isDashing)
                PlayerStamina.Instance.GenerateStaminaStep(playerScriptable.staminaPerSecond);
        }

        private float GetOverallSpeed()
        {
            if (!isOnSlope)
            {
                return moveSpeed * speedMultiplierFromDash;
            }
            else
            {
                if (isSlopeClimbing)
                {
                    return moveSpeed * speedMultiplierFromDash * (actualSlopeAngle / playerScriptable.speedDuringSlopeClimb);
                }
                else if(isDashing)
                {
                    return (moveSpeed * speedMultiplierFromDash * (actualSlopeAngle / playerScriptable.speedDuringSlopeFall)) 
                           * playerScriptable.dashInSlopeSpeedMultiplier;
                }
                else
                {
                    return moveSpeed * speedMultiplierFromDash * (actualSlopeAngle / playerScriptable.speedDuringSlopeFall);
                }
            }

            return 1f;
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

            actualSlopeAngle = Vector3.Angle(raycastSlope.normal, Vector3.up);

            if (actualSlopeAngle > playerScriptable.minSlopeDegrees) 
            {
                isOnSlope = true;

                isSlopeClimbing = raycastSlopeFront.collider;
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
        /// <param name="pm">The physical material to apply.</param>
        private void SetPhysicalMaterialCollider(PhysicMaterial pm)
        {
            //if the capsule hasn't the material yet, apply it.
            if(capsuleCollider.material != pm) capsuleCollider.material = pm;
        }
        
        
        /// <summary>
        /// Set the rigidbody drag of the player, from it's different states.
        /// </summary>
        private void SetDrag()
        {
            _rb.drag = isOnGround ? playerScriptable.groundDrag : playerScriptable.airDrag;
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
            
            //Security array to stack ground colliders detected, fixed to 10 max colliders.
            Collider[] hit = new Collider[10];
            
            //Create an offset if the player moving, for balance the FOV trick.
            var offset = isMoving ? (transform.forward * playerScriptable.groundDetectionForwardOffsetMoving) : Vector3.zero;
            
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
        
        /// <summary>
        /// Detect an edge forward from the player, for anticipate a collision and make it smooth to climb, adjusting the jump height.
        /// </summary>
        void DetectEdges()
        {
            //if the player isn't jumping, dont detect edges.
            if (!isJumping & isMoving && _rb.velocity.y > 15 && !isOnGround) return;
            
            //Security array to stack ground colliders detected, fixed to 10 max colliders.
            Collider[] hitSphere = new Collider[10];
            
            //Create a hit variable to stock the next edge surface collider.
            RaycastHit raycastHit;
            
            var selfTransform = transform;
            
            //Setting the origin position of the sphere cast for edges.
            var posSphere = (selfTransform.position + new Vector3(0, playerScriptable.edgeHeightDetection, 0)) +
                      (selfTransform.forward * playerScriptable.edgeForwardMultiplierDetection);
            
            //Setting the origin position of the raycast for edges' next surface. 
            var posRaycast = (selfTransform.position + new Vector3(0, playerScriptable.edgeHeightDetection * 1.5f, 0)) +
                            (selfTransform.forward * (playerScriptable.edgeForwardMultiplierDetection * 2f));
            
            //Detecting the forward wall.
            var detectForward = Physics.OverlapSphereNonAlloc(posSphere, playerScriptable.edgeRadiusDetection, hitSphere, groundLayer) > 0;
            
            //Detecting the next edge surface.
            var detectForwardGround = Physics.Raycast(posRaycast, Vector3.down, out raycastHit, 1.25f, groundLayer);

            //if a wall is detected and the surface above is accessible, then there is an edge.
            hasAnEdgeForward = detectForward && detectForwardGround && raycastHit.collider;

            //Apply force if the player jump and detect an edge.
            if(hasAnEdgeForward && receivedJumpInput) 
                _rb.AddForce(((Vector3.up / playerScriptable.edgeCompensationForce.x) + 
                              (transform.forward * playerScriptable.edgeCompensationForce.y)), ForceMode.Impulse);
        }
        
        #endregion
        
        //-------------------- Inputs ----------------------
        
        #region Inputs
        
        /// <summary>
        /// Make the player jump when pressing an input
        /// </summary>
        /// <param name="ctx">Automatic parameter to get the current input values.</param>
        public void JumpInput(InputAction.CallbackContext ctx)
        {
            receivedJumpInput = ctx.performed;
        }

        /// <summary>
        /// Make the player jump, at different heights if he's in coyote jump or not. 
        /// </summary>
        private void Jump()
        {
            isJumping = true;
            
            if(coyoteTimer < playerScriptable.coyoteJump - 0.1f)
                _rb.AddForce(playerScriptable.coyoteJumpForce * Vector3.up, ForceMode.Impulse);
            else
                _rb.AddForce(playerScriptable.jumpForce * Vector3.up, ForceMode.Impulse);
            
            coyoteTimer = 0f;
        }

        /// <summary>
        /// Make the player slide when pressing an input. 
        /// </summary>
        /// <param name="ctx">Automatic parameter to get the current input values.</param>
        public void Slide(InputAction.CallbackContext ctx)
        {
            isSliding = ctx.performed && isMoving && isOnGround;
        }
        
        /// <summary>
        /// Get the current inputs to set the moving direction. Added in the Player Input Component.
        /// </summary>
        /// <param name="ctx">Automatic parameter to get the current input values.</param>
        public void GetMoveInputs(InputAction.CallbackContext ctx)
        {
            //Set the current input values to the direction.
            var dir = ctx.ReadValue<Vector2>();
            direction = new Vector3(dir.x, 0, dir.y).normalized;
            
            //If the direction isn't null, set the direction not reset to direction.
            if (direction.magnitude > playerScriptable.moveThreshold) directionNotReset = direction;
        }
        
        /// <summary>
        /// Make the camera rotate from where the player look.
        /// </summary>
        /// <param name="ctx">Automatic parameter to get the current input values.</param>
        public void RotateCameraFromInput(InputAction.CallbackContext ctx)
        {
            _rotationX += -ctx.ReadValue<Vector2>().y * playerScriptable.sensibility;
            _rotationX = Mathf.Clamp(_rotationX, -playerScriptable.lookLimitY, playerScriptable.lookLimitY);
            cameraAttachPosition.localRotation = Quaternion.Euler(_rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, ctx.ReadValue<Vector2>().x * playerScriptable.sensibility, 0);
        }

        /// <summary>
        /// Make the player dash in the current moving direction. No diagonals.
        /// </summary>
        /// <param name="ctx">Automatic parameter to get the current input values.</param>
        public void DashInput(InputAction.CallbackContext ctx)
        {
            receivedDashInput = ctx.performed;
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
            
            else if(isMoving && isSliding && !isJumping && !isDashing) currentActionState = PlayerActionStates.Sliding;
            
            else if (!isSliding && isJumping && !isDashing) currentActionState = PlayerActionStates.Jumping;
                
            else if(isMoving && isDashing) currentActionState = PlayerActionStates.Dashing;

            else if (idleTimer <= 0f) currentActionState = PlayerActionStates.Idle;

            else currentActionState = PlayerActionStates.Moving;

            switch (currentActionState)
            {
                case PlayerActionStates.Idle:
                    SetPhysicalMaterialCollider(playerScriptable.frictionMaterial);
                    OnIdle();
                    break;
                case PlayerActionStates.Moving:
                    SetPhysicalMaterialCollider(playerScriptable.movingMaterial);
                    break;
                case PlayerActionStates.Sliding:
                    SetPhysicalMaterialCollider(playerScriptable.movingMaterial);
                    break;
                case PlayerActionStates.Jumping:
                    SetPhysicalMaterialCollider(playerScriptable.movingMaterial);
                    break;
                case PlayerActionStates.Dashing:
                    SetPhysicalMaterialCollider(playerScriptable.movingMaterial);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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

            //Slope Normal
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(raycastSlope.point, raycastSlope.normal * 2.5f);
            
            //Detect Edges
            Gizmos.color = Color.cyan;
            var pos = (position + new Vector3(0, playerScriptable.edgeHeightDetection, 0)) +
                      (transform.forward * (playerScriptable.edgeForwardMultiplierDetection));
            Gizmos.DrawWireSphere(pos, playerScriptable.edgeRadiusDetection);
            
            var pos2 = (position + new Vector3(0, playerScriptable.edgeHeightDetection * 1.5f, 0)) +
                      (transform.forward * (playerScriptable.edgeForwardMultiplierDetection * 2f));
            Gizmos.DrawRay(pos2, Vector3.down * 1.25f);
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

            // Display the text on the screen
            GUI.Label(rect, $"Direction : {direction}", style);
            GUI.Label(rect1, $"Direction No reset : {directionNotReset}", style);
            
            GUI.Label(rect2, $"Rigidbody Velocity : {_rb.velocity}", style);
            GUI.Label(rect3, $"Rigidbody Magnitude : {_rb.velocity.magnitude}", style);
            
            GUI.Label(rect4, $"Current State : {Convert.ToString(currentActionState)}", style);
            
            GUI.Label(rect5, $"Overall Speed : {GetOverallSpeed()}", style);
            
            GUI.Label(rect6, $"Grounded ? : {isOnGround}", BoolStyle(isOnGround));
            
            GUI.Label(rect7, $"Has an Edge Forward ? : {hasAnEdgeForward}", BoolStyle(hasAnEdgeForward));
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

