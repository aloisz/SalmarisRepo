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
            // Position
            cameraManager.transitionParent.position = Vector3.Lerp(cameraManager.transitionParent.position, cameraManager.slindingPos.position, 
                Time.deltaTime * cameraManager.so_Camera.positionOffSetSmooth);
            
            
            //Handles rotation
            float zValue = 0; // base value of cam rotation when sliding
            if (PlayerController.Instance.direction.x == 0) // Is player Not going to side left or right then add a little rotation
            {
                zValue = -4;
                cameraManager.smoothOffset = Quaternion.Slerp(cameraManager.smoothOffset, 
                    Quaternion.Euler(0, cameraManager.so_Camera.rotationOffSet.y, zValue),
                    Time.deltaTime * PlayerController.Instance.playerScriptable.smoothCameraRot);
            }
            else
            {
                cameraManager.smoothOffset = Quaternion.Slerp(cameraManager.smoothOffset, 
                    Quaternion.Euler(0, cameraManager.so_Camera.rotationOffSet.y, -PlayerController.Instance.direction.x * cameraManager.so_Camera.rotationOffSet.z * cameraManager.so_Camera.slindingRotMultiplier),
                    Time.deltaTime * PlayerController.Instance.playerScriptable.smoothCameraRot);
            }
            
            
            cameraManager.transitionParent.rotation = Quaternion.Slerp(cameraManager.transitionParent.rotation, cameraManager.playerTransform.rotation * cameraManager.smoothOffset, 
                Time.deltaTime * cameraManager.so_Camera.rotationOffSetSmooth); // PlayerController.Instance.playerScriptable.smoothCameraRot
        }
    }

}


