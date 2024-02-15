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
            transform.position = Vector3.Lerp(transform.position, cameraManager.slindingPos.position, 
                Time.deltaTime * cameraManager.rotationOffSetSmooth);
            
            
            //Handles rotation
            float zValue = 0; // base value of cam rotation when sliding
            if (PlayerController.Instance.direction.x == 0) // Is player Not going to side left or right then add a little rotation
            {
                zValue = -4;
                cameraManager.smoothOffset = Quaternion.Slerp(cameraManager.smoothOffset, 
                    Quaternion.Euler(0, cameraManager.rotationOffSet.y, zValue),
                    Time.deltaTime * PlayerController.Instance.playerScriptable.smoothCameraRot);
            }
            else
            {
                cameraManager.smoothOffset = Quaternion.Slerp(cameraManager.smoothOffset, 
                    Quaternion.Euler(0, cameraManager.rotationOffSet.y, -PlayerController.Instance.direction.x * cameraManager.rotationOffSet.z * cameraManager.slindingRotMultiplier),
                    Time.deltaTime * PlayerController.Instance.playerScriptable.smoothCameraRot);
            }
            
            
            transform.rotation = Quaternion.Slerp(transform.rotation, cameraManager.playerTransform.rotation * cameraManager.smoothOffset, 
                Time.deltaTime * cameraManager.rotationOffSetSmooth); // PlayerController.Instance.playerScriptable.smoothCameraRot
        }
    }

}


