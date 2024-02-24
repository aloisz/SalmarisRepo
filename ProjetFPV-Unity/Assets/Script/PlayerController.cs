using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
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
        
        private bool canJump = true;
        private bool canDash = true;
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
            
            ManageDashDuration();
            ManageSpeedMultiplierFromDash();
            
            SetMoveSpeed();

            RechargeStaminaFromSpeed();
            
            DetectIdling();
            
            DetectSlope();

            canDash = receivedDashInput && !isDashing && PlayerStamina.Instance.HasEnoughStamina(1);
            canJump = isOnGround && receivedJumpInput;
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

            if (!isOnSlope)
            {
                if (_rb.velocity.magnitude < playerScriptable.speedMaxToAccelerate)
                {
                    _rb.AddForce((GetOverallSpeed() * playerScriptable.accelerationMultiplier) * dir, ForceMode.Impulse);
                }
                else
                {
                    _rb.AddForce(GetOverallSpeed() * dir, ForceMode.Impulse);
                }
            }
            else
            {
                if (isSlopeClimbing)
                {
                    _rb.AddForce(dir * GetOverallSpeed() * (actualSlopeAngle / playerScriptable.speedDuringSlopeClimb), ForceMode.Impulse);
                }
                else if(isDashing)
                {
                    _rb.AddForce(dir * (GetOverallSpeed() * 2f) * (actualSlopeAngle / playerScriptable.speedDuringSlopeFall), ForceMode.Impulse);
                }
                else
                {
                    _rb.AddForce(dir * GetOverallSpeed() * (actualSlopeAngle / playerScriptable.speedDuringSlopeFall), ForceMode.Impulse);
                }
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
            return moveSpeed * speedMultiplierFromDash;
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
        void ManageGravity()
        {
            if (canApplyGravity)
            {
                var v = _rb.velocity;
                
                v.y -= Time.deltaTime * playerScriptable.gravityMultiplier;
                
                v.x = _rb.velocity.x + (DirectionFromCamera(direction).x * playerScriptable.moveAirMultiplier);
                v.z = _rb.velocity.z + (DirectionFromCamera(direction).z * playerScriptable.moveAirMultiplier);
                
                _rb.velocity = v;
            }
        }
        
        void DetectSlope()
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
        
        private void SetPhysicalMaterialCollider(PhysicMaterial pm)
        {
            capsuleCollider.material = pm;
        }
        
        private void SetDrag()
        {
            _rb.drag = isOnGround ? playerScriptable.groundDrag : playerScriptable.airDrag;
        }
        
        #endregion
        
        //-------------------- Detections ----------------------
        
        #region Detections
        void DetectGround()
        {
            isOnGround =
                Physics.CheckBox(transform.position,
                    playerScriptable.groundDetectionWidthHeightDepth,
                    Quaternion.identity, groundLayer);

            if (isOnGround && !wasOnGroundLastFrame)
            {
                OnLand();
            }
            
            wasOnGroundLastFrame = isOnGround;
        }

        private void OnLand()
        {
            isJumping = false;
            canApplyGravity = false;
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

        private void Jump()
        {
            isJumping = true;
            _rb.AddForce(playerScriptable.jumpForce * Vector3.up, ForceMode.Impulse);
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
        /// Get the current inputs to set the moving direction. Added in the Plyer Input Component.
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
        /// Make the player dash in the current moving direction. No diagonales.
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

            /*
            var v = _rb.velocity;
            v.y = 0f;
            _rb.velocity = v;*/
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
            
            else if (isMoving && !isSliding && isJumping && !isDashing) currentActionState = PlayerActionStates.Jumping;
                
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
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, playerScriptable.groundDetectionWidthHeightDepth);
            
            //Slope Detection
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(transform.position + new Vector3(0, 0.35f, 0),
                Vector3.down * playerScriptable.raycastLenghtSlopeDetection);

            //Slope Normal
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(raycastSlope.point, raycastSlope.normal * 2.5f);
        }

        private void OnGUI()
        {
            // Set up GUI style for the text
            GUIStyle style = new GUIStyle();
            style.fontSize = 24;
            style.normal.textColor = Color.white;
            
            // Set the position and size of the text
            Rect rect = new Rect(10, 10, 200, 50);
            Rect rect1 = new Rect(10, 60, 200, 50);
            Rect rect2 = new Rect(10, 110, 200, 50);
            Rect rect3 = new Rect(10, 160, 200, 50);

            // Display the text on the screen
            GUI.Label(rect, $"Direction : {direction}", style);
            GUI.Label(rect1, $"Direction No reset : {directionNotReset}", style);
            GUI.Label(rect2, $"Rigidbody Velocity : {_rb.velocity}", style);
            GUI.Label(rect3, $"Rigidbody Magnitude : {_rb.velocity.magnitude}", style);
        }

        GUIStyle BoolStyle(bool value)
        {
            var style = new GUIStyle();
            style.fontSize = 24;
            style.normal.textColor = value ? Color.green : Color.red;
            return style;
        }
        
        #endregion
        
        //-------------------- Unused ----------------------
        
        #region Unused
        
        #endregion
    }
}

