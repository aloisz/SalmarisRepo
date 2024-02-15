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
        [SerializeField] internal PlayerScriptable playerScriptable;

        [SerializeField] private Transform cameraAttachPosition;

        internal Vector3 direction;
        internal Vector3 directionNotReset;

        internal enum PlayerActionStates
        {
            Idle,
            Moving,
            Sliding,
            Jumping,
            WallRunning
        }
        internal PlayerActionStates currentActionState;

        public bool canMove = true;

        private Rigidbody _rb;

        private bool isMoving;
        private bool isSliding;
        private bool isJumping;
        private bool isWallRunning;
        
        private float rotationX;
        private Vector2 mouseInput;

        public override void Awake()
        {
            base.Awake();
            
            //get the rigidbody component.
            _rb = GetComponent<Rigidbody>();

            Cursor.lockState = CursorLockMode.Locked;
        }

        public float inputRotationY;
        float inputRotationYTarget;
        private void Update()
        {
            PlayerInputStateMachine();
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
            var dir = ctx.ReadValue<Vector2>().normalized;
            direction = new Vector3(dir.x, 0, dir.y);
            
            //If the direction isn't null, set the direction not reset to direction.
            if (direction.magnitude > 0.1f) directionNotReset = direction;
        }

        /// <summary>
        /// Move the player in the current direction value, based on scriptable parameters.
        /// </summary>
        private void Move()
        {
            //Add force to the rigidbody.
            _rb.AddForce(direction * (playerScriptable.moveSpeed * Time.deltaTime), playerScriptable.movingMethod);
        }
        
        #endregion
        
        #region Physics

        private void SetRigidbodyDrag()
        {
            //Set the rigibody's drag, based on scriptable parameters.
            _rb.drag = playerScriptable.linearDragMultiplier * playerScriptable.linearDragDeceleration;
        }
        
        #endregion

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
        
        public void RotateCameraFromInput(InputAction.CallbackContext ctx)
        {
            if (canMove)
            {
                mouseInput = ctx.ReadValue<Vector2>();
                rotationX += -ctx.ReadValue<Vector2>().y * playerScriptable.lookSpeed;
                rotationX = Mathf.Clamp(rotationX, -playerScriptable.lookLimitX, playerScriptable.lookLimitX);
                cameraAttachPosition.localRotation = Quaternion.Euler(rotationX, 0, 0);
                transform.rotation *= Quaternion.Euler(0, ctx.ReadValue<Vector2>().x * playerScriptable.lookSpeed, 0);
            }
        }
    }
}

