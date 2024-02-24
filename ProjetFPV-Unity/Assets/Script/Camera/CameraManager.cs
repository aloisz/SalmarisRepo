using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using Player;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using Player;

namespace CameraBehavior
{
    public class CameraManager : GenericSingletonClass<CameraManager>
    {
        [Header("---Scriptable---")] 
        [Expandable]public SO_Camera so_Camera;
        
        [Header("---Camera Parameter---")]
        [SerializeField] internal Transform transitionParent;
        [SerializeField] internal Transform playerTransform;
        public Transform weaponTransform;
        [SerializeField] internal bool doCameraFeel;
        
        [Header("---Sliding---")] 
        [ShowIf("doCameraFeel")][SerializeField] internal Transform slindingPos;
        
        internal Camera camera;
        internal Transform cameraTransform;
        
        internal float currentFov;
        internal Quaternion smoothOffset;
        
        // Get All Camera Component
        private CameraSliding cameraSliding;
        private CameraJumping cameraJumping;
        private CameraDash cameraDash;
        private HandSwing handSwing;
        
        internal float timer = 0;
        
        private void Awake()
        {
            cameraSliding = GetComponent<CameraSliding>();
            cameraJumping = GetComponent<CameraJumping>();
            cameraDash = GetComponent<CameraDash>();
            camera = GetComponentInChildren<Camera>();
            cameraTransform = camera.GetComponent<Transform>();
            
            currentFov = so_Camera.fovIdle;
            camera.fieldOfView = currentFov;
        }
        
        private void LateUpdate()
        {
            MovingCameraManager();
            CameraStateManagamement();
        }

        
        #region Global Management

        /// <summary>
        /// Moving the CameraManager Transform
        /// </summary>
        private void MovingCameraManager()
        {
            transform.position = Vector3.Lerp(transform.position, playerTransform.position, 
                Time.deltaTime * so_Camera.positionOffSetSmooth);
        }

        /// <summary>
        /// Manage the Camera State
        /// </summary>
        private void CameraStateManagamement()
        {
            if (!doCameraFeel)
            {
                Idle();
                return;
            }
            
            //RotateCamera();
            
            switch (PlayerController.Instance.currentActionState)
            {
                case PlayerController.PlayerActionStates.Idle:
                    Idle();
                    IdleFov();
                    ReInitialiseCameraPos();
                    break;
                
                case PlayerController.PlayerActionStates.Moving:
                    HeadBobing();
                    MovingFov();
                    MovingTransitionParent();
                    break;
                
                case PlayerController.PlayerActionStates.Sliding:
                    cameraSliding.Sliding();
                    MovingFov();
                    ReInitialiseCameraPos();
                    break;
                
                case PlayerController.PlayerActionStates.Jumping:
                    cameraJumping.Jumping();
                    ReInitialiseCameraPos();
                    break;
                
                case PlayerController.PlayerActionStates.Dashing:
                    cameraDash.Dash();
                    ReInitialiseCameraPos();
                    break;
            }
        }

        #endregion
        
        #region Idle

        /// <summary>
        /// Handles Transition Parent when Idle
        /// </summary>
        private void Idle()
        {
            timer = 0;
            transitionParent.rotation = Quaternion.Slerp(transitionParent.rotation, playerTransform.rotation, Time.deltaTime * so_Camera.rotationOffSetSmooth);
            smoothOffset = Quaternion.identity;
        }

        /// <summary>
        /// Handles The Idle FOV
        /// </summary>
        internal void IdleFov()
        {
            if (Math.Abs(currentFov - so_Camera.fovIdle) > 0.1f)
            {
                currentFov = Mathf.Lerp(currentFov, so_Camera.fovIdle, Time.deltaTime * so_Camera.timeToGetToIdleFov);
            }
            camera.fieldOfView = currentFov;
        }

        /// <summary>
        ///  Bring back the camera to correct position
        /// </summary>
        private void ReInitialiseCameraPos()
        {
            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, Vector3.zero, Time.deltaTime * so_Camera.positionOffSetSmooth);
        }

        #endregion
        
        #region Moving

        /// <summary>
        /// Handles Moving Camera, weapon Child transform for head boobing
        /// </summary>
        private void HeadBobing()
        {
            timer += Time.deltaTime * so_Camera.walkingBobbingSpeed;
            
            // Camera HeadBob
            Vector3 cameraBobbingPos = new Vector3(cameraTransform.transform.position.x, Mathf.Sin(timer) * so_Camera.cameraBobbingAmount + cameraTransform.position.y,
                cameraTransform.position.z);
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, cameraBobbingPos, timer);
            
            // Weapon HeadBob
            Vector3 weaponBobbingPos = new Vector3(weaponTransform.position.x, Mathf.Sin(timer) * so_Camera.weaponBobbingAmount + weaponTransform.position.y,
                weaponTransform.position.z);
            weaponTransform.position = Vector3.Lerp(weaponTransform.transform.position, weaponBobbingPos, timer);
        }
        
        /// <summary>
        /// Handles The FOv when PC is moving
        /// </summary>
        internal void MovingFov()
        {
            if (Math.Abs(currentFov - so_Camera.fovMoving) > 0.1f)
            {
                currentFov = Mathf.Lerp(currentFov, so_Camera.fovMoving, Time.deltaTime * so_Camera.timeToGetToMovingFov);
            }
            camera.fieldOfView = currentFov;
        }

        private void MovingTransitionParent()
        {
            transitionParent.position = Vector3.Lerp(transitionParent.position, playerTransform.position, 
                Time.deltaTime * so_Camera.positionOffSetSmooth); // PlayerController.Instance.playerScriptable.smoothCameraPos
            
            // Rotation
            float xValue = 0;
            if (PlayerController.Instance.direction.z <= 0) // Is player going backward
            {
                xValue = -PlayerController.Instance.direction.z * so_Camera.rotationOffSet.x;
            }
            
            smoothOffset = Quaternion.Slerp(smoothOffset, Quaternion.Euler(xValue, so_Camera.rotationOffSet.y, -PlayerController.Instance.direction.x * so_Camera.rotationOffSet.z),
                Time.deltaTime * so_Camera.rotationOffSetSmooth);
            
            transitionParent.rotation = Quaternion.Slerp(transitionParent.rotation, playerTransform.rotation * smoothOffset, 
                Time.deltaTime * so_Camera.rotationOffSetSmooth); // PlayerController.Instance.playerScriptable.smoothCameraRot
        }

        #endregion
        
    }
}

