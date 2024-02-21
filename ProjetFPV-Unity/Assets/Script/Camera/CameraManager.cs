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
        public SO_Camera so_Camera;
        
        [Header("---Camera Parameter---")]
        [SerializeField] internal Transform transitionParent;
        [SerializeField] internal Transform playerTransform;
        [SerializeField] internal Transform weaponTransform;
        [SerializeField] internal bool doCameraFeel;
        
        [Header("---Sliding---")] 
        [ShowIf("doCameraFeel")][SerializeField] internal Transform slindingPos;
        
        internal Camera camera;
        internal Transform cameraTransform;
        
        internal float currentFov;
        internal Vector3 defaultPos;
        internal Quaternion smoothOffset;
        
        // Get All Camera Component
        private CameraSliding cameraSliding;
        private HandSwing handSwing;
        
        internal float timer = 0;
        
        private void Awake()
        {
            cameraSliding = GetComponent<CameraSliding>();
            camera = GetComponentInChildren<Camera>();
            cameraTransform = camera.GetComponent<Transform>();
            
            currentFov = so_Camera.fovIdle;
            camera.fieldOfView = currentFov;
        }
        

        private void LateUpdate()
        {
            defaultPos = playerTransform.position;
            transitionParent.position = Vector3.Lerp(transitionParent.position, playerTransform.position, 
                Time.deltaTime * so_Camera.positionOffSetSmooth); // PlayerController.Instance.playerScriptable.smoothCameraPos
            
            if (!doCameraFeel)
            {
                Idle();
                return;
            }
            
            switch (PlayerController.Instance.currentActionState)
            {
                case PlayerController.PlayerActionStates.Idle:
                    Idle();
                    IdleFov();
                    cameraTransform.position = Vector3.Lerp(cameraTransform.position, defaultPos, Time.deltaTime * so_Camera.positionOffSetSmooth);
                    break;
                
                case PlayerController.PlayerActionStates.Moving:
                    HeadBobing();
                    MovingFov();
                    break;
                
                case PlayerController.PlayerActionStates.Sliding:
                    cameraSliding.Sliding();
                    MovingFov();
                    break;
                
                case PlayerController.PlayerActionStates.Jumping:
                    HeadBobing();
                    if(!PlayerController.Instance.isMoving)
                        IdleFov();
                    else 
                        MovingFov();
                    
                    break;
            }
        }

        #region Idle

        private void Idle()
        {
            timer = 0;
            transitionParent.rotation = Quaternion.Slerp(transitionParent.rotation, playerTransform.rotation, Time.deltaTime * so_Camera.rotationOffSetSmooth);
            smoothOffset = Quaternion.identity;
        }

        private void IdleFov()
        {
            if (Math.Abs(currentFov - so_Camera.fovIdle) > 0.1f)
            {
                currentFov = Mathf.Lerp(currentFov, so_Camera.fovIdle, Time.deltaTime * so_Camera.timeToGetToTheNewFOV);
            }
            camera.fieldOfView = currentFov;
        }

        #endregion


        #region Moving

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
            
            
            // Rotation added to all child
            float xValue = 0;
            if (PlayerController.Instance.direction.z <= 0) // Is player going backward
            {
                xValue = -PlayerController.Instance.direction.z * so_Camera.rotationOffSet.x;
            }
            
            smoothOffset = Quaternion.Slerp(smoothOffset, Quaternion.Euler(xValue, so_Camera.rotationOffSet.y, -PlayerController.Instance.direction.x * so_Camera.rotationOffSet.z),
                Time.deltaTime * PlayerController.Instance.playerScriptable.smoothCameraRot);
            
            transitionParent.rotation = Quaternion.Slerp(transitionParent.rotation, playerTransform.rotation * smoothOffset, 
                Time.deltaTime * so_Camera.rotationOffSetSmooth); // PlayerController.Instance.playerScriptable.smoothCameraRot
        }
        
        private void MovingFov()
        {
            if (Math.Abs(currentFov - so_Camera.fovMoving) > 0.1f)
            {
                currentFov = Mathf.Lerp(currentFov, so_Camera.fovMoving, Time.deltaTime * so_Camera.timeToGetToTheNewFOV);
            }
            camera.fieldOfView = currentFov;
        }

        #endregion
        
        
        
        
    }
}

