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

        public void Update()
        {
            cameraManager.walkingBobbingSpeed = 5;
        }
    }

}


