using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CameraBehavior
{
    public class HandSwing : MonoBehaviour
    {
        private CameraManager cameraManager;

        private void Awake()
        {
            cameraManager = GetComponentInParent<CameraManager>();
        }

        public void LateUpdate()
        {
            if (!cameraManager.doCameraFeel)return;
            
            float mouseX = Input.GetAxisRaw("Mouse X") * cameraManager.weaponSwaymultiplier;
            float mouseY = Input.GetAxisRaw("Mouse Y") * cameraManager.weaponSwaymultiplier;
            
            // Calculate the rotation
            Quaternion rotationX = Quaternion.AngleAxis(-mouseY, Vector3.right);
            Quaternion rotationY = Quaternion.AngleAxis(mouseX, Vector3.up);

            Quaternion targetRotation = rotationX * rotationY;

           
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, cameraManager.weaponSwaySmooth * Time.deltaTime);
        }
    }
}
