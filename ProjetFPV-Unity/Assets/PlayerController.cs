using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [RequireComponent(typeof(Rigidbody))]
    
    public class PlayerController : GenericSingletonClass<PlayerController>
    {
        internal PlayerScriptable playerScriptable;

        internal Vector2 direction;
        internal Vector2 directionNotReset;

        private Rigidbody _rb;

        public override void Awake()
        {
            base.Awake();
            //get the rigidbody component.
            _rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            Move();
        }

        private void FixedUpdate()
        {
            SetRigibodyDrag();
        }

        #region Movements
        
        /// <summary>
        /// get the current inputs to set the moving direction. Added in the Plyer Input Component.
        /// </summary>
        /// <param name="ctx">Automatic parmaeter to get the current input values.</param>
        public void GetMoveInputs(InputAction.CallbackContext ctx)
        {
            //Set the current input values to the direction.
            direction = ctx.ReadValue<Vector2>().normalized;
            
            //If the direction isn't null, set the direction not reset to direction.
            if (direction.magnitude > 0.1f) directionNotReset = direction;
        }

        private void Move()
        {
            _rb.AddForce(playerScriptable.moveSpeed * direction);
        }
        
        #endregion
        
        #region Physics

        private void SetRigibodyDrag()
        {
            _rb.drag = playerScriptable.linearDragMultiplier * playerScriptable.linearDragDeceleration;
        }
        
        #endregion
    }
}

