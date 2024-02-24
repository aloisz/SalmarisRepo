using System;
using System.Collections;
using System.Collections.Generic;
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
                cameraManager.currentFov = Mathf.Lerp(cameraManager.currentFov, cameraManager.so_Camera.fovDashing, Time.deltaTime * cameraManager.so_Camera.timeToGetToTheNewFOVDashing);
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
            cameraManager.transitionParent.rotation = Quaternion.Slerp(cameraManager.transitionParent.rotation, cameraManager.playerTransform.rotation * cameraManager.smoothOffset, 
                Time.deltaTime * cameraManager.so_Camera.rotationOffSetSmooth); // PlayerController.Instance.playerScriptable.smoothCameraRot
        }
    }
}

