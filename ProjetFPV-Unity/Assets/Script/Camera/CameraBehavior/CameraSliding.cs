using System;
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
            transform.localPosition = new Vector3(
                Mathf.Lerp(transform.localPosition.x, cameraManager.slindingPos.x, Time.deltaTime * cameraManager.walkingBobbingSpeed), 
                Mathf.Lerp(transform.localPosition.y, cameraManager.slindingPos.y, Time.deltaTime * cameraManager.walkingBobbingSpeed),
                Mathf.Lerp(transform.localPosition.z, cameraManager.slindingPos.z, Time.deltaTime * cameraManager.walkingBobbingSpeed));
        }
    }

}


