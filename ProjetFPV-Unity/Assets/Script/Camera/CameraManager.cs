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
        
        [SerializeField] internal Transform playerTransform;
        [SerializeField] internal bool doCameraFeel;
        
        // Get All Camera Component
        private CameraSliding cameraSliding;
        
        [Header("Bobbing")]
        internal float timer = 0;
        [ShowIf("doCameraFeel")][Range(0,20)][SerializeField] internal float walkingBobbingSpeed = 14f;
        [ShowIf("doCameraFeel")][Range(-.1f,.1f)][SerializeField] internal float bobbingAmount = 0.05f;
        
        internal Vector3 defaultPos;
        internal Quaternion smoothOffset;

        [Header("Moving")]
        [ShowIf("doCameraFeel")][SerializeField] internal Vector3 rotationOffSet;

        [Header("Sliding")] 
        [ShowIf("doCameraFeel")][SerializeField] internal Transform slindingPos;
        [ShowIf("doCameraFeel")][SerializeField] internal float slindingRotMultiplier = 3f;
        private void Awake()
        {
            cameraSliding = GetComponent<CameraSliding>();
            defaultPos = playerTransform.position;
        }

        private void ChangeState()
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                PlayerController.Instance.currentActionState = PlayerController.PlayerActionStates.Sliding;
            }
        }

        private void LateUpdate()
        {
            transform.position = Vector3.Lerp(transform.position, playerTransform.position, 
                Time.deltaTime * PlayerController.Instance.playerScriptable.smoothCameraPos);

            if (!doCameraFeel)
            {
                Idle();
                return;
            }

            ChangeState();
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
            transform.rotation = Quaternion.Slerp(transform.rotation, playerTransform.rotation, Time.deltaTime * PlayerController.Instance.playerScriptable.smoothCameraRot);
            smoothOffset = Quaternion.identity;
            
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
                xValue = -PlayerController.Instance.direction.z * rotationOffSet.x;
            }
            
            smoothOffset = Quaternion.Slerp(smoothOffset, Quaternion.Euler(xValue, rotationOffSet.y, -PlayerController.Instance.direction.x * rotationOffSet.z),
                Time.deltaTime * PlayerController.Instance.playerScriptable.smoothCameraRot);
            
            transform.rotation = Quaternion.Slerp(transform.rotation, playerTransform.rotation * smoothOffset, 
                Time.deltaTime * PlayerController.Instance.playerScriptable.smoothCameraRot);
        }
    }
}

