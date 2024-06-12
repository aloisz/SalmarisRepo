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
using UnityEditor;

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
        
        // Get All Camera Component
        internal CameraSliding cameraSliding;
        internal CameraJumping cameraJumping;
        private CameraDash cameraDash;
        internal HandSwing handSwing;
        internal CameraFrustumCulling cameraFrustumCulling;

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
            cameraFrustumCulling = GetComponent<CameraFrustumCulling>();

            Instance = this;
            
            currentFov = so_Camera.fovIdle;
            camera.fieldOfView = currentFov;

            actualglobalCameraRot = globalCameraRot;
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
            if (!mustAvoid)
            {
                //var YImpact = (cameraJumping.jumpingImpactOnLanding.Evaluate(PlayerController.Instance._rb.velocity.y) );
                
                transform.position = Vector3.Lerp(transform.position, playerTransform.position, 
                    Time.deltaTime * (so_Camera.positionOffSetSmooth));
                
                transform.rotation = Quaternion.Euler(transform.eulerAngles.x , playerTransform.eulerAngles.y,transform.eulerAngles.z );
                
                
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
            //cameraJumping.DisplayCameraShake();

            handSwing.CameraImpact();
            cameraJumping.ImpactWhenLanding();

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
            //smoothOffset = Quaternion.identity;
            smoothOffset = Quaternion.Slerp(smoothOffset, Quaternion.Euler(0, so_Camera.rotationOffSet.y,
                    -PlayerController.Instance.direction.x * so_Camera.rotationOffSet.z),
                Time.deltaTime * so_Camera.rotationOffSetSmooth);
            
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

        #region Reset Camera Rotation

        private void ResetCameraRotation()
        {
            smoothOffset =
                Quaternion.Slerp(smoothOffset, Quaternion.identity, 
                    Time.deltaTime * so_Camera.rotationOffSetSmooth);
        }

        #endregion
        
        #region CameraBounds
        
        [Header("CameraBounds")] 
        /*[SerializeField] internal float maxRayDistance;*/
        [SerializeField] internal LayerMask obstacleLayer;
        private bool mustAvoid;
        /*[SerializeField] internal float timeToReset;
        [SerializeField] internal float interpolationTime;
        private float timeSpent;
        [SerializeField] internal float newInterpolatedValue;*/
        
        
        public float collisionOffset = 0.1f; // Offset to maintain from the wall
        public float repulseForce = 1f; // Strength of the repulsion force
        

        private void CheckForCameraBounds()
        {
            /*Debug.DrawRay(transitionParent.position + Vector3.up, transitionParent.forward * maxRayDistance, Color.green);
            
            if(mustAvoid) return;
            RaycastHit hit;
            if (Physics.Raycast(transitionParent.position, transitionParent.forward, out hit, maxRayDistance,
                    obstacleLayer))
            {
                if ( (obstacleLayer & (1 << hit.transform.gameObject.layer)) != 0) // Check the layer 
                {
                    mustAvoid = true;
                }
                newInterpolatedValue = Mathf.Lerp(newInterpolatedValue, interpolationTime, Vector3.Distance(hit.point, PlayerController.Instance.transform.position));
            }
            else
            {
                newInterpolatedValue = so_Camera.positionOffSetSmooth;
            }*/
            
            Vector3 desiredWorldPosition = cameraTransform.localPosition;
            Debug.DrawRay(transitionParent.position, transitionParent.forward *(collisionOffset) , Color.green);

            if (PlayerController.Instance._rb.velocity.magnitude < 30) return;
            RaycastHit hit;
            if (Physics.Raycast(transitionParent.position, transitionParent.forward, out hit, 
                    collisionOffset, obstacleLayer))
            {
                Vector3 directionToWall = (hit.point - desiredWorldPosition).normalized;
                desiredWorldPosition -= directionToWall * repulseForce;
                
                cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, desiredWorldPosition, Time.deltaTime * repulseForce);
            }
            
            
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
        
#if UNITY_EDITOR    
        private void OnDrawGizmos()
        {
            /*Handles.color = Color.blue;
            Handles.DrawLine(transitionParent.position, transitionParent.position + (transitionParent.forward * maxRayDistance), 5);

            Handles.color = Color.green;
            Handles.DrawLine(transitionParent.position, transitionParent.position + (-transitionParent.up * maxRayDistance), 5);*/

            /*Gizmos.color = Color.red;
            Gizmos.DrawCube(desiredCameraPos, Vector3.one * .5f);*/
        }
#endif

    }
}

