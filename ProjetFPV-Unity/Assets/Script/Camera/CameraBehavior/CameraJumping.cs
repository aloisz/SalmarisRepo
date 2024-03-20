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

        private void Awake()
        {
            cameraManager = GetComponent<CameraManager>();
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
                Time.deltaTime * cameraManager.so_Camera.positionOffSetSmooth);
        }

        private void Rotation()
        {
            cameraManager.transitionParent.rotation = Quaternion.Slerp(cameraManager.transitionParent.rotation, cameraManager.playerTransform.rotation * cameraManager.smoothOffset, 
                Time.deltaTime * cameraManager.so_Camera.rotationOffSetSmooth); // PlayerController.Instance.playerScriptable.smoothCameraRot
            
            
            /*if (PlayerController.Instance._rb.velocity.magnitude > 2)
           {
                cameraManager.timer += Time.deltaTime * cameraManager.so_Camera.JumpingBobbingSpeed;
            
            
                // Camera HeadBob
                Quaternion cameraBobbingPos = 
                    new Quaternion(cameraManager.cameraTransform.transform.rotation.x + Mathf.Cos(cameraManager.timer) * cameraManager.so_Camera.cameraJumpingBobbingAmount , 
                        cameraManager.cameraTransform.rotation.y + Mathf.Sin(cameraManager.timer) * cameraManager.so_Camera.cameraJumpingBobbingAmount ,
                        cameraManager.cameraTransform.rotation.z, 0);
            
                cameraManager.cameraTransform.rotation = Quaternion.Slerp(cameraManager.transitionParent.rotation, cameraBobbingPos, cameraManager.timer);
            
                // Weapon HeadBob
                Vector3 weaponBobbingPos = new Vector3(
                    cameraManager.weaponTransform.position.x, 
                    Mathf.Sin(cameraManager.timer) * cameraManager.so_Camera.cameraJumpingBobbingAmount * 0.1f + cameraManager.weaponTransform.position.y,
                    cameraManager.weaponTransform.position.z);
            
                cameraManager.weaponTransform.position = Vector3.Lerp(cameraManager.weaponTransform.transform.position, weaponBobbingPos, cameraManager.timer);
                
                
                
                cameraManager.smoothOffset = Quaternion.Slerp(cameraManager.smoothOffset, 
                    Quaternion.Euler(
                        0, 
                        cameraManager.so_Camera.rotationOffSet.y,
                        -PlayerController.Instance.direction.x * (Mathf.Cos(cameraManager.timer) * cameraManager.so_Camera.cameraJumpingBobbingAmount)), // * cameraManager.so_Camera.rotationOffSet.z
                    
                    Time.deltaTime * cameraManager.so_Camera.rotationOffSetSmooth);
                
                cameraManager.transitionParent.rotation = Quaternion.Slerp(cameraManager.transitionParent.rotation, cameraManager.playerTransform.rotation * cameraManager.smoothOffset, 
                    Time.deltaTime * cameraManager.so_Camera.rotationOffSetSmooth); // PlayerController.Instance.playerScriptable.smoothCameraRot
           }*/
        }
        
        private float timer = 0;
        
        public void HandlesHighSpeed()
        {
            HighSpeedMultiplicator();
            if (!PlayerController.Instance.isOnGround)
            {
                timer += Time.deltaTime * cameraManager.so_Camera.JumpingBobbingSpeed;
                
                cameraManager.smoothOffset = Quaternion.Slerp(cameraManager.smoothOffset, 
                    Quaternion.Euler(
                        0, 
                        cameraManager.so_Camera.rotationOffSet.y,
                        cameraManager.so_Camera.rotationOffSet.z * (Mathf.Cos(timer) * (cameraManager.so_Camera.cameraJumpingBobbingAmount * multiplicator))), 
                    
                    Time.deltaTime * cameraManager.so_Camera.rotationOffSetSmooth);
                
                cameraManager.transitionParent.rotation = Quaternion.Slerp(cameraManager.transitionParent.rotation, cameraManager.playerTransform.rotation * cameraManager.smoothOffset, 
                    Time.deltaTime * cameraManager.so_Camera.rotationOffSetSmooth); // PlayerController.Instance.playerScriptable.smoothCameraRot
            }
            
            Debug.Log(multiplicator);
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


