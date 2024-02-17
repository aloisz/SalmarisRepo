using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
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
        [ShowNonSerializedField] internal Vector3 direction;
        [ShowNonSerializedField] internal Vector3 directionNotReset;
        
        [ShowNonSerializedField] private float _velocity;
        private float _rotationX;
        
        private const float _gravity = -9.81f;
        
        //---------------------------------------
        
        [Header("States")]
        [ShowNonSerializedField] internal PlayerActionStates currentActionState;
        
        [ShowNonSerializedField] private bool isOnGround;
        [ShowNonSerializedField] private bool isMoving;
        [ShowNonSerializedField] private bool isSliding;
        [ShowNonSerializedField] private bool isJumping;
        [ShowNonSerializedField] private bool isWallRunning;
        [ShowNonSerializedField] private bool wallOnLeft;
        [ShowNonSerializedField] private bool wallOnRight;
        [ShowNonSerializedField] private bool canJump;
        
        //---------------------------------------

        [Header("Detection")] 
        [SerializeField] private LayerMask groundLayer;
        
        internal enum PlayerActionStates
        {
            Idle,
            Moving,
            Sliding,
            Jumping,
            WallRunning
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
            DetectWalls();
            Move();
        }

        #region Movements
        
        /// <summary>
        /// get the current inputs to set the moving direction. Added in the Plyer Input Component.
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
        /// Move the player in the current direction value, based on scriptable parameters.
        /// </summary>
        private void Move()
        {
            //_rb.velocity = Vector3.Lerp(_rb.velocity, _rb.velocity + DirectionFromCamera(direction) * playerScriptable.moveSpeed, playerScriptable.changeDirectionSpeed * Time.deltaTime);

            var dir = DirectionFromCamera(direction) * playerScriptable.moveSpeed;
            var targetVelocity = new Vector3(dir.x, _rb.velocity.y, dir.z);
            
            _rb.velocity = Vector3.MoveTowards(_rb.velocity, targetVelocity, Time.deltaTime * playerScriptable.accelerationSpeed);
            if (!isOnGround)
            {
                var v = _rb.velocity;
                v.y -= Time.deltaTime * playerScriptable.gravityMultiplier;
                
                v.x = _rb.velocity.x + (DirectionFromCamera(direction).x * playerScriptable.moveAirMultiplier);
                v.z = _rb.velocity.z + (DirectionFromCamera(direction).z * playerScriptable.moveAirMultiplier);
                
                _rb.velocity = v;
            }
        }
        
        #endregion
        
        #region Camera
        
        public void RotateCameraFromInput(InputAction.CallbackContext ctx)
        {
            if (canMove)
            {
                _rotationX += -ctx.ReadValue<Vector2>().y * playerScriptable.lookSpeed;
                _rotationX = Mathf.Clamp(_rotationX, -playerScriptable.lookLimitX, playerScriptable.lookLimitX);
                cameraAttachPosition.localRotation = Quaternion.Euler(_rotationX, 0, 0);
                transform.rotation *= Quaternion.Euler(0, ctx.ReadValue<Vector2>().x * playerScriptable.lookSpeed, 0);
            }
        }

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

        void DetectGround()
        {
            isOnGround =
                Physics.CheckBox(transform.position,
                    playerScriptable.groundDetectionWidthHeightDepth,
                    Quaternion.identity, groundLayer);
        }

        void DetectWalls()
        {
            var offset = playerScriptable.wallDetectionOffset;
            
            wallOnLeft = Physics.CheckBox(transform.position - new Vector3(offset.x, -offset.y, offset.z),
                playerScriptable.wallDetectionWidthAndHeight / 2,
                Quaternion.identity, groundLayer);
            
            wallOnRight = Physics.CheckBox(transform.position + new Vector3(offset.x, offset.y, offset.z),
                playerScriptable.wallDetectionWidthAndHeight / 2,
                Quaternion.identity, groundLayer);

            _rb.useGravity = !(wallOnLeft || wallOnRight);
        }

        public void Jump(InputAction.CallbackContext ctx)
        {
            if (ctx.performed && isOnGround)
            {
                isJumping = true;
                _rb.AddForce(playerScriptable.jumpForce * Vector3.up, ForceMode.Impulse);
            }
            
            if (ctx.performed && wallOnLeft)
            {
                isJumping = true;
                _rb.AddForce(playerScriptable.wallJumpForce * (transform.forward + -transform.right), ForceMode.Impulse);
            }
            
            if (ctx.performed && wallOnRight)
            {
                isJumping = true;
                _rb.AddForce(playerScriptable.wallJumpForce * (transform.forward + transform.right), ForceMode.Impulse);
            }
        }

        void PlayerInputStateMachine()
        {
            isMoving = direction.magnitude > playerScriptable.moveThreshold;
            
            isSliding = false; //TODO
            isJumping = false; //TODO
            isWallRunning = false; //TODO

            if (isMoving && !isSliding) currentActionState = PlayerActionStates.Moving;
            
            else if(isMoving && isSliding) currentActionState = PlayerActionStates.Sliding;
            
            else if(isJumping) currentActionState = PlayerActionStates.Jumping;
            else if (isWallRunning) currentActionState = PlayerActionStates.WallRunning;

            else currentActionState = PlayerActionStates.Idle;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, playerScriptable.groundDetectionWidthHeightDepth);
            
            Gizmos.color = Color.green;
            var offset = playerScriptable.wallDetectionOffset;
            Gizmos.DrawWireCube(transform.position - new Vector3(offset.x, -offset.y, offset.z), 
                playerScriptable.wallDetectionWidthAndHeight);
            
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(transform.position + new Vector3(offset.x, offset.y, offset.z), 
                playerScriptable.wallDetectionWidthAndHeight);
        }
    }
}

