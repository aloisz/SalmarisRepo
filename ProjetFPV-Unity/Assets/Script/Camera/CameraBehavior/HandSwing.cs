using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Weapon;
using Random = UnityEngine.Random;

namespace CameraBehavior
{
    public class HandSwing : MonoBehaviour
    {
        private CameraManager cameraManager;
        private Vector3 basePos;
        internal float JumpingOffSetY;
        
        // Gun
        private float lastfired;
        [SerializeField] private WeaponManager weapon;
        
        
        private void Awake()
        {
            cameraManager = GetComponentInParent<CameraManager>();
            basePos = transform.localPosition;
        }

        private void Start()
        {
            weapon.OnShoot += Shoot;
        }

        public void LateUpdate()
        {
            if (!cameraManager.doCameraFeel)return;

            if (!PlayerInputs.Instance.weaponInputs.enabled) return;
            
            float mouseX = Input.GetAxisRaw("Mouse X") * -cameraManager.so_Camera.weaponSwaymultiplier;
            float mouseY = Input.GetAxisRaw("Mouse Y") * cameraManager.so_Camera.weaponSwaymultiplier;
            
            // Calculate the rotation
            Quaternion rotationX = Quaternion.AngleAxis(-mouseY, Vector3.right);
            Quaternion rotationY = Quaternion.AngleAxis(mouseX, Vector3.up);

            Quaternion targetRotation = rotationX * rotationY;

            // weapon rotation 
            transform.localRotation = cameraManager.cameraSliding.timeElapsed > 0 ? 
                Quaternion.Slerp(transform.localRotation, targetRotation * Quaternion.AngleAxis(40, Vector3.forward), cameraManager.so_Camera.weaponSwaySmooth * Time.deltaTime ) : 
                Quaternion.Slerp(transform.localRotation, targetRotation, cameraManager.so_Camera.weaponSwaySmooth * Time.deltaTime );
            
            // weapon position
            transform.localPosition = Vector3.Lerp(transform.localPosition, basePos + new Vector3(0,JumpingOffSetY,0), cameraManager.so_Camera.weaponSwaySmooth * Time.deltaTime);
            
            if (weapon.isShooting)
            {
                if (Time.time - lastfired > 1 / weapon.so_Weapon.weaponMode[(int)weapon.actualWeaponModeIndex].fireRate)
                {
                    Shoot();
                }
            }
        }

        private void Shoot()
        {
            lastfired = Time.time;
            float angleX = Random.Range(weapon.so_Weapon.weaponMode[(int)weapon.actualWeaponModeIndex].weaponRecoilRotx.x, 
                weapon.so_Weapon.weaponMode[(int)weapon.actualWeaponModeIndex].weaponRecoilRotx.y);
            
            float angleZ = Random.Range(weapon.so_Weapon.weaponMode[(int)weapon.actualWeaponModeIndex].weaponRecoilRotZ.x,
                weapon.so_Weapon.weaponMode[(int)weapon.actualWeaponModeIndex].weaponRecoilRotZ.y);
            
            Quaternion shootingRot = transform.localRotation * Quaternion.Euler(-angleX, angleZ, 0);
            transform.localRotation = 
                Quaternion.Slerp(transform.localRotation, shootingRot, cameraManager.so_Camera.rotationOffSetSmooth * Time.deltaTime);
            
            float zRecoil = weapon.so_Weapon.weaponMode[(int)weapon.actualWeaponModeIndex].weaponRecoilPosZ;
            transform.localPosition = 
                Vector3.Lerp(transform.localPosition, transform.localPosition + new Vector3(0,0,-zRecoil), cameraManager.so_Camera.positionOffSetSmooth * Time.deltaTime);//cameraManager.so_Camera.positionOffSetSmooth * Time.deltaTime

        }   
    }
}
