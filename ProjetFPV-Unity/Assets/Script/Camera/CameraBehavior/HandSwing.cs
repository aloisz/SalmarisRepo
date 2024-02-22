using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CameraBehavior
{
    public class HandSwing : MonoBehaviour
    {
        private CameraManager cameraManager;
        private Vector3 basePos;
        
        // Gun
        private float lastfired;
        [SerializeField] private float FireRate = 10;
        

        private void Awake()
        {
            cameraManager = GetComponentInParent<CameraManager>();
            basePos = transform.localPosition;
        }

        public void LateUpdate()
        {
            if (!cameraManager.doCameraFeel)return;
            
            float mouseX = Input.GetAxisRaw("Mouse X") * cameraManager.so_Camera.weaponSwaymultiplier;
            float mouseY = Input.GetAxisRaw("Mouse Y") * cameraManager.so_Camera.weaponSwaymultiplier;
            
            // Calculate the rotation
            Quaternion rotationX = Quaternion.AngleAxis(-mouseY, Vector3.right);
            Quaternion rotationY = Quaternion.AngleAxis(mouseX, Vector3.up);

            Quaternion targetRotation = rotationX * rotationY;
            
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, cameraManager.so_Camera.weaponSwaySmooth * Time.deltaTime );
            transform.localPosition = Vector3.Lerp(transform.localPosition, basePos, cameraManager.so_Camera.weaponSwaySmooth * Time.deltaTime);
            
            if (Input.GetKey(KeyCode.Mouse0))
            {
                if (Time.time - lastfired > 1 / FireRate)
                {
                    //Shoot();
                }
            }
        }

        private void Shoot()
        {
            lastfired = Time.time;
            float angleX = Random.Range(3, 30);
            float angleZ = Random.Range(-3,3);
            
            //transform.localRotation *= Quaternion.Euler(-angleX, angleZ, 0);
            Quaternion shootingRot = transform.localRotation * Quaternion.Euler(-angleX, angleZ, 0);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, shootingRot, 1500 * Time.deltaTime);
            
            transform.localPosition = Vector3.Lerp(transform.localPosition, transform.localPosition + new Vector3(0,0,-0.5f), 500 * Time.deltaTime );

        }   
    }
}
