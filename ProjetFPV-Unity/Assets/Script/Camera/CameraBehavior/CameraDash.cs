using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;


namespace CameraBehavior
{
    public class CameraDash : MonoBehaviour
    {
        private CameraManager cameraManager;

        private void Awake()
        {
            cameraManager = GetComponent<CameraManager>();
        }
        
        public void Dash()
        {
            DashingFov();
            Position();
            Rotation();
        }
        private void DashingFov()
        {
            if (Math.Abs(cameraManager.currentFov - cameraManager.so_Camera.fovDashing) > 0.1f)
            {
                cameraManager.currentFov = Mathf.Lerp(cameraManager.currentFov, cameraManager.so_Camera.fovDashing, Time.deltaTime * cameraManager.so_Camera.timeToGetToDashingFov);
            }
            cameraManager.camera.fieldOfView = cameraManager.currentFov;
        }
        
        private void Position()
        {
            // Position
            cameraManager.transitionParent.position = Vector3.Lerp(cameraManager.transitionParent.position, cameraManager.playerTransform.position, 
                Time.deltaTime * cameraManager.so_Camera.positionOffSetSmooth);
        }

        private void Rotation()
        {
            float zValue = 0; // base value of cam rotation when sliding
            if (PlayerController.Instance.direction.x == 0) // Is player Not going to side left or right then add a little rotation
            {
                cameraManager.smoothOffset = Quaternion.Slerp(cameraManager.smoothOffset, 
                    Quaternion.Euler(0, 0, zValue),
                    Time.deltaTime * cameraManager.so_Camera.rotationOffSetSmooth);
            }
            else
            {
                cameraManager.smoothOffset = Quaternion.Slerp(cameraManager.smoothOffset, 
                    Quaternion.Euler(0, 0, -PlayerController.Instance.direction.x * cameraManager.so_Camera.dashingRotationOffSet.z * cameraManager.so_Camera.dashingRotMultiplier),
                    Time.deltaTime * cameraManager.so_Camera.rotationOffSetSmooth);
            }
            
            
            cameraManager.transitionParent.rotation = Quaternion.Slerp(cameraManager.transitionParent.rotation, cameraManager.playerTransform.rotation * cameraManager.smoothOffset, 
                Time.deltaTime * cameraManager.so_Camera.rotationOffSetSmooth); // PlayerController.Instance.playerScriptable.smoothCameraRot
        }
    }
}

