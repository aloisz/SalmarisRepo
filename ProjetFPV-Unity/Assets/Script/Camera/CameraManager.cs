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
    public class CameraManager : MonoBehaviour
    {
        [Header("---Scriptable---")] 
        [Expandable]public SO_Camera so_Camera;

        [Header("---Camera Parameter---")] 
        [SerializeField] internal float globalCameraRot;
        internal float actualglobalCameraRot;
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

        private Plane[] cameraFrustum;
        private Bounds bounds;
        
        // Get All Camera Component
        internal CameraSliding cameraSliding;
        private CameraJumping cameraJumping;
        private CameraDash cameraDash;
        internal HandSwing handSwing;

        private bool isCommingBackFromEffect;
        
        internal float timer = 0;

        public static CameraManager Instance;
        
        private void Awake()
        {
            cameraSliding = GetComponent<CameraSliding>();
            cameraJumping = GetComponent<CameraJumping>();
            cameraDash = GetComponent<CameraDash>();
            camera = GetComponentInChildren<Camera>();
            cameraTransform = camera.GetComponent<Transform>();
            handSwing = GetComponentInChildren<HandSwing>();

            Instance = this;
            
            currentFov = so_Camera.fovIdle;
            camera.fieldOfView = currentFov;

            actualglobalCameraRot = globalCameraRot;

            //bounds = GetComponent<Collider>().bounds;
        }
        
        private void LateUpdate()
        {
            MovingCameraManager();
            CameraStateManagamement();
            
        }

        public void Update()
        {
            ObstacleAvoidance();
        }


        #region Global Management

        /// <summary>
        /// Moving the CameraManager Transform
        /// </summary>
        private void MovingCameraManager()
        {
            if (!mustAvoid)
            {
                transform.position = Vector3.Lerp(transform.position, playerTransform.position, 
                    Time.deltaTime * (so_Camera.positionOffSetSmooth - 
                                      (cameraJumping.jumpingImpact.Evaluate(PlayerController.Instance._rb.velocity.magnitude) * 5)));
            }
            else
            {
                LogicWhenAvoid();
            }
            
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
                    actualglobalCameraRot = so_Camera.rotationOffSetSmooth;
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

            // Fix the different rotation smoothness
            if (Math.Abs(actualglobalCameraRot - globalCameraRot) > 0.1f) 
            {
                actualglobalCameraRot = Mathf.Lerp(actualglobalCameraRot, globalCameraRot,
                    Time.deltaTime * so_Camera.rotationOffSetSmooth);
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
            smoothOffset = Quaternion.identity;
            transitionParent.rotation = Quaternion.Slerp(transitionParent.rotation, playerTransform.rotation * smoothOffset , Time.deltaTime * actualglobalCameraRot); 
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
            if (isCommingBackFromEffect)
            {
                isCommingBackFromEffect = false;
                transitionParent.position = Vector3.Lerp(transitionParent.position, playerTransform.position, 
                    Time.deltaTime * so_Camera.positionOffSetSmooth);
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
        }

        private void MovingTransitionParent()
        {
            transitionParent.position = Vector3.Lerp(transitionParent.position, playerTransform.position, 
                Time.deltaTime * so_Camera.positionOffSetSmooth); 
            
            // Rotation
            float xValue = 0;
            if (PlayerController.Instance.direction.z <= 0) // Is player going backward
            {
                xValue = -PlayerController.Instance.direction.z * so_Camera.rotationOffSet.x;
            }
            
            smoothOffset = Quaternion.Slerp(smoothOffset, Quaternion.Euler(xValue, so_Camera.rotationOffSet.y, -PlayerController.Instance.direction.x * so_Camera.rotationOffSet.z),
                Time.deltaTime * so_Camera.rotationOffSetSmooth);
            
            transitionParent.rotation = Quaternion.Slerp(transitionParent.rotation, playerTransform.rotation * smoothOffset, 
                Time.deltaTime * actualglobalCameraRot); 
        }

        #endregion

        #region ObstacleAvoidance

        [Header("ObstacleAvoidance")] 
        [SerializeField] internal float maxRayDistance;
        [SerializeField] internal LayerMask obstacleLayer;
        private bool mustAvoid;
        [SerializeField] internal float timeToReset;
        [SerializeField] internal float interpolationTime;
        private float timeSpent;

        private void ObstacleAvoidance()
        {
            Debug.DrawRay(transitionParent.position + Vector3.up, transitionParent.forward * maxRayDistance, Color.green);
            
            if(mustAvoid) return;
            RaycastHit hit;
            if (Physics.Raycast(transitionParent.position, transitionParent.forward, out hit, maxRayDistance,
                    obstacleLayer))
            {
                if ( (obstacleLayer & (1 << hit.transform.gameObject.layer)) != 0) // Check the layer 
                {
                    mustAvoid = true;
                }
            }
        }

        private void LogicWhenAvoid()
        {
            transform.position = Vector3.Lerp(transform.position, playerTransform.position, 
                Time.deltaTime * interpolationTime);
            timeSpent += Time.deltaTime;
                
            if (!(timeSpent > timeToReset)) return;
            mustAvoid = false;
            timeSpent = 0;
        }

        #endregion


        private void CalculateFrustum()
        {
            /*cameraFrustum = GeometryUtility.CalculateFrustumPlanes(camera);
            if(GeometryUtility.TestPlanesAABB(cameraFrustum, bounds)
            {
                
            }*/
        }
        
    }
}

