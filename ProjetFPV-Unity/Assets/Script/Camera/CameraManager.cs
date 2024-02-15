using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace CameraBehavior
{
    public class CameraManager : MonoBehaviour
    {
        [SerializeField] internal PlayerCameraState cameraState;
        
        [SerializeField] internal bool doCameraFeel;
        
        // Get All CameraComponent
        private CameraSliding cameraSliding;
        
        [Header("Bobbing")]
        [ShowIf("doCameraFeel")][Range(0,20)][SerializeField] internal float walkingBobbingSpeed = 14f;
        [ShowIf("doCameraFeel")][Range(-.1f,.1f)][SerializeField] internal float bobbingAmount = 0.05f;
        
        internal Vector3 defaultPos;
        
        float timer = 0;

        [Header("Sliding")] 
        [SerializeField] internal Vector3 slindingPos;

        private void Awake()
        {
            cameraSliding = GetComponent<CameraSliding>();
            defaultPos = transform.position;
        }

        private void LateUpdate()
        {
            if(!doCameraFeel) return;
            switch (cameraState)
            {
                case PlayerCameraState.Idle:
                    Idle();
                    break;
                
                case PlayerCameraState.Moving:
                    HeadBobing();
                    break;
                
                case PlayerCameraState.Sliding:
                    cameraSliding.Sliding();
                    break;
                
                case PlayerCameraState.Jumping:
                    break;
                
                case PlayerCameraState.WallRuning:
                    break;
            }
        }

        private void Idle()
        {
            timer = 0;
            transform.localPosition = new Vector3(
                Mathf.Lerp(transform.localPosition.x, defaultPos.x, Time.deltaTime * walkingBobbingSpeed), 
                Mathf.Lerp(transform.localPosition.y, defaultPos.y, Time.deltaTime * walkingBobbingSpeed),
                transform.localPosition.z);
        }
        
        private void HeadBobing()
        {
            timer += Time.deltaTime * walkingBobbingSpeed;
            transform.localPosition = new Vector3(transform.localPosition.x, defaultPos.y + Mathf.Sin(timer) * bobbingAmount, transform.localPosition.z);
        }
    }
}

public enum PlayerCameraState{
    Idle,
    Moving,
    Sliding,
    Jumping,
    WallRuning
}

