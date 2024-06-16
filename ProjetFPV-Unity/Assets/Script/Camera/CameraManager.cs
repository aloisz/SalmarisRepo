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
using Script;
using UnityEditor;

namespace CameraBehavior
{
    public class CameraManager : GenericSingletonClass<CameraManager>, IDestroyInstance
    {
        [Header("---Scriptable---")] 
        [Expandable]public SO_Camera so_Camera;

        [Header("---Camera Parameter---")]
        [SerializeField] internal float actualpositionOffSetSmooth;
        [SerializeField] private AnimationCurve posOffSetSmoothCurve;
        [SerializeField] internal Transform transitionParent;
        [SerializeField] internal Transform playerTransform;
        public Transform weaponTransform;
        [SerializeField] internal bool doCameraFeel;
        
        
        [Header("---Sliding---")] 
        [ShowIf("doCameraFeel")][SerializeField] internal Transform slindingPos;
        
        internal Camera camera;
        internal Transform cameraTransform;

        private GameObject goCameraCullingWeapon;
        internal Camera cameraCullingWeapon;
        
        internal float currentFov;  
        internal Quaternion smoothOffset;
        
        // Get All Camera Component
        internal CameraSliding cameraSliding;
        internal CameraJumping cameraJumping;
        private CameraDash cameraDash;
        internal HandSwing handSwing;
        internal CameraFrustumCulling cameraFrustumCulling;

        private bool isCommingBackFromEffect;
        internal float timer = 0;
        
        
        public override void Awake()
        {
            base.Awake();
            
            cameraSliding = GetComponent<CameraSliding>();
            cameraJumping = GetComponent<CameraJumping>();
            cameraDash = GetComponent<CameraDash>();
            camera = GetComponentInChildren<Camera>();
            goCameraCullingWeapon = GameObject.Find("CameraGunCulling"); // get the camera culling child of Main Camera
            cameraCullingWeapon = goCameraCullingWeapon.GetComponent<Camera>();
            cameraTransform = camera.GetComponent<Transform>();
            handSwing = GetComponentInChildren<HandSwing>();
            cameraFrustumCulling = GetComponent<CameraFrustumCulling>();
            
            currentFov = so_Camera.fovIdle;
            camera.fieldOfView = currentFov;

            actualpositionOffSetSmooth = so_Camera.positionOffSetSmooth;
            DontDestroyOnLoad(gameObject);
        }
        
        private void LateUpdate()
        {
            MovingCameraManager();
            CameraStateManagamement();
            ResetCameraRotation();

        }

        public void Update()
        {
            CheckForCameraBounds();
        }


        #region Global Management

        /// <summary>
        /// Moving the CameraManager Transform
        /// </summary>
        private void MovingCameraManager()
        {
            //var YImpact = (cameraJumping.jumpingImpactOnLanding.Evaluate(PlayerController.Instance._rb.velocity.y) );
            
            transform.position = Vector3.Lerp(transform.position, playerTransform.position, 
                Time.deltaTime * (actualpositionOffSetSmooth));
            
            transform.rotation = Quaternion.Euler(transform.eulerAngles.x , playerTransform.eulerAngles.y,transform.eulerAngles.z );
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
                    isCommingBackFromEffect = true;
                    cameraSliding.ResetTimer();
                    break;
                
                case PlayerController.PlayerActionStates.Moving:
                    HeadBobing();
                    MovingFov();
                    MovingTransitionParent();
                    cameraSliding.ResetTimer();
                    break;
                
                case PlayerController.PlayerActionStates.Sliding:
                    cameraSliding.Sliding();
                    ReInitialiseCameraPos();
                    break;
                
                case PlayerController.PlayerActionStates.Jumping:
                    cameraJumping.Jumping();
                    ReInitialiseCameraPos();
                    cameraSliding.ResetTimer();
                    break;
                
                case PlayerController.PlayerActionStates.Dashing:
                    cameraDash.Dash();
                    ReInitialiseCameraPos();
                    cameraSliding.ResetTimer();
                    break;
            }
            cameraJumping.HandlesHighSpeed();
            //cameraJumping.DisplayCameraShake();

            handSwing.CameraImpact();
            cameraJumping.ImpactWhenLanding();
            
        }

        #endregion
        
        #region Idle

        /// <summary>
        /// Handles Transition Parent when Idle
        /// </summary>
        private void Idle()
        {
            timer = 0;
            //smoothOffset = Quaternion.identity;
            smoothOffset = Quaternion.Slerp(smoothOffset, Quaternion.Euler(0, so_Camera.rotationOffSet.y,
                    -PlayerController.Instance.direction.x * so_Camera.rotationOffSet.z),
                Time.deltaTime * so_Camera.rotationOffSetSmooth);
            
            transitionParent.rotation = Quaternion.Slerp(transitionParent.rotation, playerTransform.rotation * smoothOffset , Time.deltaTime * so_Camera.rotationOffSetSmooth); 
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
            cameraCullingWeapon.fieldOfView = currentFov;
        }

        /// <summary>
        ///  Bring back the camera to correct position
        /// </summary>
        private void ReInitialiseCameraPos()
        {
            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, Vector3.zero, Time.deltaTime * actualpositionOffSetSmooth);
            if (isCommingBackFromEffect)
            {
                isCommingBackFromEffect = false;
                transitionParent.position = Vector3.Lerp(transitionParent.position, playerTransform.position, 
                    Time.deltaTime * actualpositionOffSetSmooth);
            }
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
            cameraCullingWeapon.fieldOfView = currentFov;
        }
        
        private void MovingTransitionParent()
        {
            transitionParent.position = Vector3.Lerp(transitionParent.position, playerTransform.position, 
                Time.deltaTime * actualpositionOffSetSmooth); 
            
            // transitionParent Rotation
            float xValue = 0;
            if (PlayerController.Instance.direction.z <= 0) // Is player going backward
            {
                xValue = -PlayerController.Instance.direction.z * so_Camera.rotationOffSet.x;
            }
            
            smoothOffset = Quaternion.Slerp(smoothOffset, Quaternion.Euler(xValue, so_Camera.rotationOffSet.y, -PlayerController.Instance.direction.x * so_Camera.rotationOffSet.z),
                Time.deltaTime * so_Camera.rotationOffSetSmooth);
            
            transitionParent.rotation = Quaternion.Slerp(transitionParent.rotation, playerTransform.rotation * smoothOffset, 
                Time.deltaTime * so_Camera.rotationOffSetSmooth); 
            
            // Weapon Rotation
            float zValue = 0f;
            zValue = PlayerController.Instance.direction.x == 1 ? 100 : 50;
            
            Quaternion weaponBobbingRot = Quaternion.Euler
                (weaponTransform.transform.localRotation.x, so_Camera.rotationOffSet.y, PlayerController.Instance.direction.x * (zValue));
            
            weaponTransform.localRotation = Quaternion.Slerp(weaponTransform.transform.localRotation, weaponBobbingRot, Time.deltaTime * 1);
        }

        #endregion

        #region Reset Camera Rotation

        private void ResetCameraRotation()
        {
            smoothOffset =
                Quaternion.Slerp(smoothOffset, Quaternion.identity, 
                    Time.deltaTime * so_Camera.rotationOffSetSmooth);
                
            CameraShake.Instance.cameraShakePos.rotation = Quaternion.Slerp(CameraShake.Instance.cameraShakePos.rotation,
            playerTransform.rotation * smoothOffset,
            Time.deltaTime * so_Camera.rotationOffSetSmooth);
        }

        #endregion
        
        #region CameraBounds
        private void CheckForCameraBounds()
        {
            actualpositionOffSetSmooth = Mathf.Lerp(actualpositionOffSetSmooth,
                posOffSetSmoothCurve.Evaluate(PlayerController.Instance._rb.velocity.magnitude), Time.deltaTime);
        }

        private void LogicWhenAvoid()
        {
            /*transform.position = Vector3.Lerp(transform.position, playerTransform.position, 
                Time.deltaTime * interpolationTime);
            timeSpent += Time.deltaTime;
                
            if (!(timeSpent > timeToReset)) return;
            mustAvoid = false;
            timeSpent = 0;*/
        }
        

        #endregion

        public void DestroyInstance()
        {
            Destroy(gameObject);
        }
    }
}

