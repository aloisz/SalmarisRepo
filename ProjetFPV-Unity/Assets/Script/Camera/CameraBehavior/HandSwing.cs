using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
            WeaponState.Instance.barbatos.OnHudShoot += Shoot;
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
            
        }

        private void Shoot()
        {
           isShooting = true;
        }

        private bool isShooting;
        private float timeElapsed;
        private float heightX = 0f;
        private float heightY = 0f;
        private float heightZ = 0f;
        
        public void CameraImpact()
        {
            if(!isShooting) return;
            var weaponSO = weapon.so_Weapon.weaponMode[(int)weapon.actualWeaponModeIndex];
            
            if (timeElapsed > weaponSO.weaponRecoilDuration)
            {
                timeElapsed = 0;
                isShooting = false;
            }
            
            timeElapsed += Time.deltaTime * 1; 
            
            var transitionParentPos = cameraManager.cameraTransform.localPosition;
            var heightTime = weaponSO.weaponRecoilCameraOffsetCurve.Evaluate(timeElapsed);

            if (timeElapsed < weaponSO.weaponRecoilDuration / 2)
            {
                heightX = Mathf.Lerp(transitionParentPos.x, transitionParentPos.x + weaponSO.weaponRecoilCameraOffset.x, heightTime); 
                heightY = Mathf.Lerp(transitionParentPos.y, transitionParentPos.y + weaponSO.weaponRecoilCameraOffset.y, heightTime); 
                heightZ = Mathf.Lerp(transitionParentPos.z, transitionParentPos.z + weaponSO.weaponRecoilCameraOffset.z, heightTime);
            }
            else
            {
                heightX = Mathf.Lerp(transitionParentPos.x, 0, heightTime); 
                heightY = Mathf.Lerp(transitionParentPos.y, 0, heightTime); 
                heightZ = Mathf.Lerp(transitionParentPos.z, 0, heightTime);
            }
            
            cameraManager.cameraTransform.localPosition = new Vector3(heightX,heightY, heightZ);
            WeaponImpact(weaponSO);
        }

        private void WeaponImpact(SO_WeaponMode weaponSo)
        {
            lastfired = Time.time;
            float angleX = Random.Range(weaponSo.weaponRecoilRotx.x, 
                weaponSo.weaponRecoilRotx.y);
            
            float angleY = Random.Range(weaponSo.weaponRecoilRotY.x, 
                weaponSo.weaponRecoilRotY.y);
            
            float angleZ = Random.Range(weaponSo.weaponRecoilRotZ.x,
                weaponSo.weaponRecoilRotZ.y);
            
            float posX = Random.Range(weaponSo.weaponRecoilPosX.x,
                weaponSo.weaponRecoilPosX.y);
            
            float posY = Random.Range(weaponSo.weaponRecoilPosY.x,
                weaponSo.weaponRecoilPosY.y);
            
            float posZ = Random.Range(weaponSo.weaponRecoilPosZ.x,
                weaponSo.weaponRecoilPosZ.y);

            Vector3 basePos = transform.localPosition;
            
            if (timeElapsed < weaponSo.weaponRecoilDuration / 2)
            {
                basePos = transform.localPosition;
                Quaternion shootingRot = transform.localRotation * Quaternion.Euler(-angleX, angleY, angleZ);
                transform.localRotation = 
                    Quaternion.Slerp(transform.localRotation, shootingRot, cameraManager.so_Camera.rotationOffSetSmooth * Time.deltaTime);
                
                transform.localPosition = 
                    Vector3.Lerp(transform.localPosition, transform.localPosition + new Vector3(posX,posY,posZ), timeElapsed);//cameraManager.so_Camera.positionOffSetSmooth * Time.deltaTime

            }
            else
            {
                transform.localRotation = 
                    Quaternion.Slerp(transform.localRotation, Quaternion.identity, timeElapsed);
                
                transform.localPosition = 
                    Vector3.Lerp(transform.localPosition, basePos, timeElapsed);
            }
            
            /*Quaternion shootingRot = transform.localRotation * Quaternion.Euler(-angleX, angleZ, 0);
            transform.localRotation = 
                Quaternion.Slerp(transform.localRotation, shootingRot, cameraManager.so_Camera.rotationOffSetSmooth * Time.deltaTime);
            
            float zRecoil = weapon.so_Weapon.weaponMode[(int)weapon.actualWeaponModeIndex].weaponRecoilPosZ;
            transform.localPosition = 
                Vector3.Lerp(transform.localPosition, transform.localPosition + new Vector3(0,0,-zRecoil), cameraManager.so_Camera.positionOffSetSmooth * Time.deltaTime);//cameraManager.so_Camera.positionOffSetSmooth * Time.deltaTime
*/
        }
    }
}
