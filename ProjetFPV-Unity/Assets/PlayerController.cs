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
        [Header("Management")]
        public bool canMove = true;
        
        //---------------------------------------
        
        [Header("Components")]
        [SerializeField] internal PlayerScriptable playerScriptable;
        [SerializeField] private Transform cameraAttachPosition;
        
        private Rigidbody _rb;
        
        //---------------------------------------

        [Header("Values")]
        public float intertiaMultiplier = 1f;
        
        [ShowNonSerializedField] internal Vector3 direction;
        [ShowNonSerializedField] internal Vector3 directionNotReset;

        private float moveSpeed;
        
        private float _velocity;
        private float _rotationX;
        private float dashTimer;
        
        private const float _gravity = -9.81f;
        
        //---------------------------------------
        
        [Header("States")]
        [ShowNonSerializedField] internal PlayerActionStates currentActionState;
        
        public bool isOnGround;
        public bool wasOnGroundLastFrame;
        public bool isMoving;
        public bool isSliding;
        public bool isJumping;
        public bool isDashing;
        
        [ShowNonSerializedField] private bool canJump = true;
        [ShowNonSerializedField] private bool canDash = true;
        [ShowNonSerializedField] private bool canApplyGravity = true;
        
        [ShowNonSerializedField] private bool receivedJumpInput;
        [ShowNonSerializedField] private bool receivedDashInput;
        
        //---------------------------------------

        [Header("Detection")] 
        [SerializeField] private LayerMask groundLayer;
        
        internal enum PlayerActionStates
        {
            Idle,
            Moving,
            Sliding,
            Jumping,
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
            
            ManageDashTimer();

            SetMoveSpeed();
        }
        
        private void FixedUpdate()
        {
            Move();
            ManageGravity();
            
            _rb.drag = isOnGround ? playerScriptable.groundDrag : playerScriptable.airDrag;
            
            if(isOnGround && receivedJumpInput) Jump();
            if(receivedDashInput && !isDashing && isMoving && canDash) Dash();
        }

        private void ManageDashTimer()
        {
            dashTimer.DecreaseTimerIfPositive();
            if (dashTimer <= 0f)
            {
                canApplyGravity = true;
                isDashing = false;
            }
        }
        
        //-------------------- Movements ----------------------

        #region Movements
        
        /// <summary>
        /// Move the player in the current direction value, based on scriptable parameters.
        /// </summary>
        private void Move()
        {
            var dir = DirectionFromCamera(direction).normalized * moveSpeed;
            
            #region unused
            /*var targetVelocity = new Vector3(dir.x, _rb.velocity.y, dir.z);
            _rb.velocity = Vector3.MoveTowards(_rb.velocity, targetVelocity * intertiaMultiplier, 
                Time.deltaTime * playerScriptable.accelerationSpeed);*/
            #endregion
            
            _rb.AddForce(moveSpeed * dir, ForceMode.Impulse);
        }
        
        private void SetMoveSpeed()
        {
            moveSpeed = isOnGround ? playerScriptable.moveSpeed : playerScriptable.moveSpeed / 2.5f;
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
                
                //if(!wallOnLeft || !wallOnRight)
                v.y -= Time.deltaTime * playerScriptable.gravityMultiplier;
                
                v.x = _rb.velocity.x + (DirectionFromCamera(direction).x * playerScriptable.moveAirMultiplier);
                v.z = _rb.velocity.z + (DirectionFromCamera(direction).z * playerScriptable.moveAirMultiplier);
                
                _rb.velocity = v;
            }
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
            canDash = true;
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
            _rotationX += -ctx.ReadValue<Vector2>().y * playerScriptable.lookSpeed;
            _rotationX = Mathf.Clamp(_rotationX, -playerScriptable.lookLimitX, playerScriptable.lookLimitX);
            cameraAttachPosition.localRotation = Quaternion.Euler(_rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, ctx.ReadValue<Vector2>().x * playerScriptable.lookSpeed, 0);
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
            dashTimer = playerScriptable.dashDuration;

            isDashing = true;
            canDash = false;
                
            var v = _rb.velocity;
            v.y = 0f;
            _rb.velocity = v;
        }
        
        #endregion
        
        //-------------------- State Machine ----------------------

        #region StateMachine
        void PlayerInputStateMachine()
        {
            isMoving = direction.magnitude > playerScriptable.moveThreshold;

            if (isMoving && !isSliding && !isJumping) currentActionState = PlayerActionStates.Moving;
            
            else if(isMoving && isSliding && !isJumping) currentActionState = PlayerActionStates.Sliding;
            
            else if(isMoving && !isSliding&& isJumping) currentActionState = PlayerActionStates.Jumping;

            else currentActionState = PlayerActionStates.Idle;
        }
        
        #endregion
        
        //-------------------- Debug ----------------------

        #region Debug
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, playerScriptable.groundDetectionWidthHeightDepth);
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

            // Display the text on the screen
            GUI.Label(rect, $"Direction : {direction}", style);
            GUI.Label(rect1, $"Direction No reset : {directionNotReset}", style);
            GUI.Label(rect2, $"Rigidbody Velocity : {_rb.velocity}", style);
        }
        
        #endregion
        
        //-------------------- Unused ----------------------
        
        #region Unused
        
        #endregion
    }
}

