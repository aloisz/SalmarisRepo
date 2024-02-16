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
        internal Camera camera;
        internal Transform cameraTransform;
        [SerializeField] internal Transform weaponTransform;
        
        [SerializeField] internal bool doCameraFeel;
        
        // Get All Camera Component
        private CameraSliding cameraSliding;
        private HandSwing handSwing;
        
        internal float timer = 0;
        [Header("Bobbing")]
        [ShowIf("doCameraFeel")] [Range(0, 20)] [SerializeField] internal float walkingBobbingSpeed;
        [ShowIf("doCameraFeel")] [Range(-.01f, .01f)] [SerializeField] internal float cameraBobbingAmount;
        [ShowIf("doCameraFeel")] [Range(-.03f, .03f)] [SerializeField] internal float weaponBobbingAmount;
        
        internal Vector3 defaultPos;
        internal Quaternion smoothOffset;

        [Header("Moving")]
        [ShowIf("doCameraFeel")][SerializeField] internal Vector3 rotationOffSet;
        [ShowIf("doCameraFeel")][SerializeField] internal float rotationOffSetSmooth;

        [Header("Sliding")] 
        [ShowIf("doCameraFeel")][SerializeField] internal Transform slindingPos;
        [ShowIf("doCameraFeel")][SerializeField] internal float slindingRotMultiplier = 3f;
        
        [Header("Weapon Sway Settings")]
        [ShowIf("doCameraFeel")][SerializeField] internal float weaponSwaySmooth;
        [ShowIf("doCameraFeel")][SerializeField] internal float weaponSwaymultiplier;
        
        private void Awake()
        {
            cameraSliding = GetComponent<CameraSliding>();
            camera = GetComponentInChildren<Camera>();
            cameraTransform = camera.GetComponent<Transform>();
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
            transform.rotation = Quaternion.Slerp(transform.rotation, playerTransform.rotation, Time.deltaTime * rotationOffSetSmooth);
            smoothOffset = Quaternion.identity;
        }
        
        private void HeadBobing()
        {
            timer += Time.deltaTime * walkingBobbingSpeed;
            
            // Camera HeadBob
            Vector3 cameraBobbingPos = new Vector3(cameraTransform.transform.position.x, Mathf.Sin(timer) * cameraBobbingAmount + cameraTransform.position.y,
                cameraTransform.position.z);
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, cameraBobbingPos, timer);
            
            // Weapon HeadBob
            Vector3 weaponBobbingPos = new Vector3(weaponTransform.position.x, Mathf.Sin(timer) * weaponBobbingAmount + weaponTransform.position.y,
                weaponTransform.position.z);
            weaponTransform.position = Vector3.Lerp(weaponTransform.transform.position, weaponBobbingPos, timer);
            
            
            // Rotation added to all child
            float xValue = 0;
            if (PlayerController.Instance.direction.z <= 0) // Is player going backward
            {
                xValue = -PlayerController.Instance.direction.z * rotationOffSet.x;
            }
            
            smoothOffset = Quaternion.Slerp(smoothOffset, Quaternion.Euler(xValue, rotationOffSet.y, -PlayerController.Instance.direction.x * rotationOffSet.z),
                Time.deltaTime * PlayerController.Instance.playerScriptable.smoothCameraRot);
            
            transform.rotation = Quaternion.Slerp(transform.rotation, playerTransform.rotation * smoothOffset, 
                Time.deltaTime * rotationOffSetSmooth); // PlayerController.Instance.playerScriptable.smoothCameraRot
        }
    }
}

