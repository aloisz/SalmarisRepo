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
        [SerializeField] private Transform playerTransform;
        [SerializeField] internal bool doCameraFeel;
        
        // Get All Camera Component
        private CameraSliding cameraSliding;
        
        [Header("Bobbing")]
        [ShowIf("doCameraFeel")][Range(0,20)][SerializeField] internal float walkingBobbingSpeed = 14f;
        [ShowIf("doCameraFeel")][Range(-.1f,.1f)][SerializeField] internal float bobbingAmount = 0.05f;
        
        internal Vector3 defaultPos;
        
        [Header("Moving")]
        float timer = 0;
        [ShowIf("doCameraFeel")][SerializeField] internal Vector3 offSet;
        [ShowIf("doCameraFeel")][Range(0,50)][SerializeField] private float smoothRotation;

        [Header("Sliding")] 
        [ShowIf("doCameraFeel")][SerializeField] internal Vector3 slindingPos;

        private void Awake()
        {
            cameraSliding = GetComponent<CameraSliding>();
            defaultPos = playerTransform.position;
        }

        private void LateUpdate()
        {
            transform.position = Vector3.Lerp(transform.position, playerTransform.position, 
                Time.deltaTime * PlayerController.Instance.playerScriptable.smoothCameraRot);
            
            if(!doCameraFeel) return;
            switch (PlayerController.Instance.currentActionState)
            {
                case PlayerController.PlayerActionStates.Idle:
                    Idle();
                    break;
                
                case PlayerController.PlayerActionStates.Moving:
                    HeadBobing();
                    break;
                
                case PlayerController.PlayerActionStates.Sliding:
                    cameraSliding.Sliding();
                    break;
                
                case PlayerController.PlayerActionStates.Jumping:
                    break;
                
                case PlayerController.PlayerActionStates.WallRunning:
                    break;
            }
        }

        private void Idle()
        {
            timer = 0;
            
            
            transform.rotation = Quaternion.Slerp(transform.rotation, playerTransform.rotation, Time.deltaTime * smoothRotation);
            /*float camRotX = Mathf.Lerp(transform.rotation.eulerAngles.x, playerTransform.rotation.eulerAngles.x, Time.deltaTime * smoothRotation);
            float camRotY = Mathf.Lerp(transform.rotation.eulerAngles.y, playerTransform.rotation.eulerAngles.y, Time.deltaTime * PlayerController.Instance.playerScriptable.smoothCameraRot);
            float camRotZ = Mathf.Lerp(transform.rotation.eulerAngles.z, playerTransform.rotation.eulerAngles.z, Time.deltaTime * smoothRotation);
            
            transform.rotation = Quaternion.Euler(camRotX, camRotY, camRotZ);*/
        }
        
        private void HeadBobing()
        {
            timer += Time.deltaTime * walkingBobbingSpeed;
            Vector3 headBobbingPos = new Vector3(transform.position.x, Mathf.Sin(timer) * bobbingAmount + transform.localPosition.y,
                transform.localPosition.z);
            transform.position = Vector3.Lerp(transform.position, headBobbingPos, timer );

            float xValue = 0;
            if (PlayerController.Instance.direction.z <= 0) // Is player going backward
            {
                xValue = -PlayerController.Instance.direction.z * offSet.x;
            }
            transform.rotation = Quaternion.Slerp(transform.rotation, playerTransform.rotation * Quaternion.Euler(xValue, offSet.y, -PlayerController.Instance.direction.x * offSet.z), 
                Time.deltaTime * smoothRotation);
        }
    }
}

