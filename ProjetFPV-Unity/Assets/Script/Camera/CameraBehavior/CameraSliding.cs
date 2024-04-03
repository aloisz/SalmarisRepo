using System;
using Player;
using UnityEngine;

namespace CameraBehavior
{
    public class CameraSliding : MonoBehaviour
    {
        
        private CameraManager cameraManager;

        private void Awake()
        {
            cameraManager = GetComponent<CameraManager>();
        }

        public void Sliding()
        {
            Position();
            Rotation();
            SlidingFov();
        }

        private void Position()
        {
            // Position
            cameraManager.transitionParent.position = Vector3.Lerp(cameraManager.transitionParent.position, cameraManager.slindingPos.position, 
                Time.deltaTime * cameraManager.so_Camera.positionOffSetSmooth);
        }

        private void Rotation()
        {
            //Handles rotation
            float zValue = 0; // base value of cam rotation when sliding
            if (PlayerController.Instance.direction.x == 0) // Is player Not going to side left or right then add a little rotation
            {
                zValue = -4;
                cameraManager.smoothOffset = Quaternion.Slerp(cameraManager.smoothOffset, 
                    Quaternion.Euler(0, cameraManager.so_Camera.rotationOffSet.y, zValue),
                    Time.deltaTime * cameraManager.so_Camera.rotationOffSetSmooth);
            }
            else
            {
                cameraManager.smoothOffset = Quaternion.Slerp(cameraManager.smoothOffset, 
                    Quaternion.Euler(0, cameraManager.so_Camera.rotationOffSet.y, -PlayerController.Instance.direction.x * cameraManager.so_Camera.rotationOffSet.z * cameraManager.so_Camera.slindingRotMultiplier),
                    Time.deltaTime * cameraManager.so_Camera.rotationOffSetSmooth);
            }
            
            
            cameraManager.transitionParent.rotation = Quaternion.Slerp(cameraManager.transitionParent.rotation, cameraManager.playerTransform.rotation * cameraManager.smoothOffset, 
                Time.deltaTime * cameraManager.so_Camera.rotationOffSetSmooth); // cameraManager.so_Camera.rotationOffSetSmooth
        }
        
        internal void SlidingFov()
        {
            if (Math.Abs(cameraManager.currentFov - cameraManager.so_Camera.fovSliding) > 0.1f)
            {
                cameraManager.currentFov = Mathf.Lerp(cameraManager.currentFov, cameraManager.so_Camera.fovSliding, 
                    Time.deltaTime * cameraManager.so_Camera.timeToGetToSlidingFov);
            }
            cameraManager.camera.fieldOfView = cameraManager.currentFov;
        }
    }

}


