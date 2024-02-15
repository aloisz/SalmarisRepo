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
        
        private const float _gravity = 9.81f;
        
        //---------------------------------------
        
        [Header("States")]
        [ShowNonSerializedField] internal PlayerActionStates currentActionState;
        
        [ShowNonSerializedField] private bool isOnGround;
        [ShowNonSerializedField] private bool isMoving;
        [ShowNonSerializedField] private bool isSliding;
        [ShowNonSerializedField] private bool isJumping;
        [ShowNonSerializedField] private bool isWallRunning;
        
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
            ApplyGravity();
        }

        private void FixedUpdate()
        {
            Move();
            SetRigidbodyDrag();
        }

        #region Movements
        
        /// <summary>
        /// get the current inputs to set the moving direction. Added in the Plyer Input Component.
        /// </summary>
        /// <param name="ctx">Automatic parmaeter to get the current input values.</param>
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
            //Add force to the rigidbody.
            _rb.AddForce(DirectionFromCamera(direction) * (playerScriptable.moveSpeed * Time.deltaTime), playerScriptable.movingMethod);
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

        public void Jump(InputAction.CallbackContext ctx)
        {
            if (ctx.performed && isOnGround)
            {
                _rb.velocity = new Vector3(_rb.velocity.x, playerScriptable.jumpForce, _rb.velocity.z);
                jumpTimer = 0f;
            }
        }

        private float jumpTimer;
        private void ApplyGravity()
        {
            if (isOnGround)
            {
                _velocity = -1f;
            }
            else
            {
                //_velocity -= (_gravity * playerScriptable.gravityJumpModify.Evaluate(jumpTimer * playerScriptable.jumpCurveSpeed)) * Time.deltaTime;
                
                var v = _rb.velocity;
                v.y += _velocity;
                _rb.velocity = v;

                jumpTimer += Time.deltaTime;
            }
        }
        
        private void SetRigidbodyDrag()
        {
            //Set the rigibody's drag, based on scriptable parameters.
            _rb.drag = playerScriptable.linearDragMultiplier * playerScriptable.linearDragDeceleration;
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
        }
    }
}

