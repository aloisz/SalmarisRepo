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

            /*if (PlayerController.Instance._rb.velocity.magnitude < 40) return;
                
            cameraManager.timer += Time.deltaTime * cameraManager.so_Camera.JumpingBobbingSpeed;
            // Camera HeadBob
            Vector3 cameraBobbingPos = 
                new Vector3(cameraManager.cameraTransform.transform.position.x + Mathf.Cos(cameraManager.timer) * cameraManager.so_Camera.cameraJumpingBobbingAmount * multiplicator, 
                    cameraManager.cameraTransform.position.y + Mathf.Sin(cameraManager.timer) * cameraManager.so_Camera.cameraJumpingBobbingAmount * multiplicator,
                    cameraManager.cameraTransform.position.z);
            
            cameraManager.cameraTransform.position = Vector3.Lerp(cameraManager.cameraTransform.position, cameraBobbingPos, cameraManager.timer);
            
                
            // Weapon HeadBob
            Vector3 weaponBobbingPos = new Vector3(
                cameraManager.weaponTransform.position.x, 
                Mathf.Sin(cameraManager.timer) * cameraManager.so_Camera.cameraJumpingBobbingAmount * 0.1f + cameraManager.weaponTransform.position.y,
                cameraManager.weaponTransform.position.z);
            
            cameraManager.weaponTransform.position = Vector3.Lerp(cameraManager.weaponTransform.transform.position, weaponBobbingPos, cameraManager.timer);*/
        }

        private void Rotation()
        {
            cameraManager.transitionParent.rotation = Quaternion.Slerp(cameraManager.transitionParent.rotation, cameraManager.playerTransform.rotation * cameraManager.smoothOffset, 
                Time.deltaTime * cameraManager.so_Camera.rotationOffSetSmooth); // PlayerController.Instance.playerScriptable.smoothCameraRot
        }

        private void Update()
        {
           /* if (PlayerController.Instance.currentActionState == PlayerController.PlayerActionStates.Jumping)
            {
                if(PlayerController.Instance._rb.velocity.magnitude > 40) multiplicator += Time.deltaTime * 1;
            }
            else
            {
                if(multiplicator >= 0) multiplicator -= Time.deltaTime * 10;
            }
            Debug.Log(multiplicator);*/
        }
    }
}


