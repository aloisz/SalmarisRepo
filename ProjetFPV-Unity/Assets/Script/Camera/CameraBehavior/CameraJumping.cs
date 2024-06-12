using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;

namespace CameraBehavior
{
    public class CameraJumping : MonoBehaviour
    {
        private CameraManager cameraManager;
        [SerializeField] internal AnimationCurve jumpingImpact;
        [SerializeField] internal AnimationCurve jumpingImpactOnLanding;
        [SerializeField] internal AnimationCurve jumpingImpactHandSwing;

        [Header("Jump Impact")] 
        [SerializeField] internal Transform cameraPos;
        [SerializeField] internal Transform baseCameraPos;
        [SerializeField] internal Transform maxcameraPos;
        [SerializeField] private float YImpact;
        [SerializeField] private float YImpactDeMultiplier;
        [SerializeField] private float jumpHandSwingSpeed = 0.5f;
        
        private void Awake()
        {
            cameraManager = GetComponent<CameraManager>();
        }


        public void ImpactWhenLanding()
        {
            if (!PlayerController.Instance.isOnGround)
            {
                var localYImpact = jumpingImpactOnLanding.Evaluate(PlayerController.Instance._rb.velocity.y);
                if (localYImpact > YImpact)
                {
                    YImpact = localYImpact;
                    //Debug.Log(YImpact);
                }
            }
            else
            {
                if (YImpact > 0)
                {
                    YImpact -= Time.deltaTime * YImpactDeMultiplier;
                }
            }
            
            if(!PlayerController.Instance.isOnGround) return;
            Vector3 offSet = new Vector3(0, -YImpact, 0);
            
            cameraPos.position = 
                Vector3.Lerp(cameraPos.position,  baseCameraPos.position + offSet, 
                    Time.deltaTime * (cameraManager.actualpositionOffSetSmooth));
        }

        public void Jumping()
        {
            if(!PlayerController.Instance.isMoving)
                cameraManager.IdleFov();
            else 
                cameraManager.MovingFov();
            
            Position();
            Rotation();
        }

        private float multiplicator = 0;
        private void Position()
        {
            // Position
            cameraManager.transitionParent.position = Vector3.Lerp(cameraManager.transitionParent.position, cameraManager.playerTransform.position, 
                Time.deltaTime * cameraManager.actualpositionOffSetSmooth);
        }

        private void Rotation()
        {
            cameraManager.transitionParent.rotation = Quaternion.Slerp(cameraManager.transitionParent.rotation, cameraManager.playerTransform.rotation * cameraManager.smoothOffset, 
                Time.deltaTime * cameraManager.so_Camera.rotationOffSetSmooth); // PlayerController.Instance.playerScriptable.smoothCameraRot
        }
        
        private float timer = 0;
        
        public void HandlesHighSpeed()
        {
            HighSpeedMultiplicator();
            if (!PlayerController.Instance.isOnGround)
            {
                timer += Time.deltaTime * cameraManager.so_Camera.JumpingBobbingSpeed;

                cameraManager.smoothOffset =
                    Quaternion.Slerp(cameraManager.smoothOffset, Quaternion.Euler(0, cameraManager.so_Camera.rotationOffSet.y,
                            cameraManager.so_Camera.rotationOffSet.z * (Mathf.Cos(timer) * (cameraManager.so_Camera.cameraJumpingBobbingAmount *
                                                 jumpingImpact.Evaluate((Mathf.Abs(PlayerController.Instance._rb.velocity.y)))))),

                        Time.deltaTime * cameraManager.so_Camera.rotationOffSetSmooth);


                cameraManager.transitionParent.rotation = Quaternion.Slerp(cameraManager.transitionParent.rotation,
                    cameraManager.playerTransform.rotation * cameraManager.smoothOffset,
                    Time.deltaTime *
                    cameraManager.so_Camera
                        .rotationOffSetSmooth); // PlayerController.Instance.playerScriptable.smoothCameraRot

                // weapon smooth
                cameraManager.handSwing.JumpingOffSetY = Mathf.Lerp(cameraManager.handSwing.JumpingOffSetY,
                    jumpingImpactHandSwing.Evaluate(PlayerController.Instance._rb.velocity.y), 
                    Time.deltaTime * jumpHandSwingSpeed);
            }
            else cameraManager.handSwing.JumpingOffSetY = 0f; // weapon smooth
        }


        private void HighSpeedMultiplicator()
        {
            if (!PlayerController.Instance.isOnGround)
            {
                if (PlayerController.Instance._rb.velocity.magnitude > cameraManager.so_Camera.highSpeedEnabled && multiplicator <= cameraManager.so_Camera.highSpeedMaxMultiplierValue)
                {
                    multiplicator += Time.deltaTime * cameraManager.so_Camera.highSpeedMultiplier;
                }
            }
            else
            {
                if(multiplicator >= 0) multiplicator -= Time.deltaTime * cameraManager.so_Camera.highSpeedDeMultiplier;
            }
        }
    }
}


