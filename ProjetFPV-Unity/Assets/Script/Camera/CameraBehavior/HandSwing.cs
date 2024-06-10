using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using Player;
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
        [SerializeField]internal float JumpingOffSetY;
        
        // Gun  
        private float lastfired;
        [SerializeField] private WeaponManager weapon;
        
        [Header("Sliding Rotation")]
        [SerializeField] private float slidingRotX = 25;
        [SerializeField] private float slidingRotY = 25;
        [SerializeField] private float slidingRotFromDirection = 25;
        [SerializeField] private float slidingRotFromDirectionMultiplierAbsX = 4;

        [Header("Sliding Shaking")] 
        [SerializeField] private float shakeTimer;
        [SerializeField] private float shakeTimerSpeed;
        
        [SerializeField] [MinMaxSlider(-2, 2)]
        private Vector2 slidingShakePosX;
        
        [SerializeField] [MinMaxSlider(-2, 2)]
        private Vector2 slidingShakePosY;
        
        [SerializeField] [MinMaxSlider(-2, 2)]
        private Vector2 slidingShakePosZ;

        [Header("Weapon reloading")]
        [SerializeField] private float reloadingRotX;
        [SerializeField] private float reloadingRotY;
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

            float playerDirX = PlayerController.Instance.direction.x;
            Quaternion rotationSwayDirection = PlayerController.Instance.isSliding ?
                Quaternion.Euler(0,0, playerDirX * (slidingRotFromDirection * (playerDirX > 0.5f ? slidingRotFromDirectionMultiplierAbsX : 1f))) : Quaternion.Euler(0,0,0);

            Quaternion targetRotation = rotationX * rotationY;

            // weapon rotation 
            transform.localRotation = cameraManager.cameraSliding.timeElapsed > 0 && !weapon.isReloading ? 
                Quaternion.Slerp(transform.localRotation, 
                    targetRotation * Quaternion.AngleAxis(slidingRotX, Vector3.forward) * Quaternion.AngleAxis(slidingRotY, Vector3.up) * rotationSwayDirection, 
                    cameraManager.so_Camera.weaponSwaySmooth * Time.deltaTime ) :
                
                Quaternion.Slerp(transform.localRotation, targetRotation, cameraManager.so_Camera.weaponSwaySmooth * Time.deltaTime );
            
            AnimateWeaponReloading();
            
            // weapon position
            transform.localPosition = Vector3.Lerp(transform.localPosition, 
                basePos + new Vector3(0  ,JumpingOffSetY ,0) + GetShakingVector3(), 
                cameraManager.so_Camera.weaponSwaySmooth * Time.deltaTime);
            
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

            Rotation(weaponSO);
            
            WeaponImpact(weaponSO);
        }
        
        private void Rotation(SO_WeaponMode weaponSO)
        {
            if (timeElapsed < weaponSO.weaponRecoilDuration / 1.5f)
            {
                cameraManager.smoothOffset = Quaternion.Slerp(
                        
                    cameraManager.smoothOffset, 
                    
                    Quaternion.Euler(
                        -1 * weaponSO.weaponRecoilCameraOffsetRotXYZ.x, 
                        weaponSO.weaponRecoilCameraOffsetRotXYZ.y, 
                        -1 * weaponSO.weaponRecoilCameraOffsetRotXYZ.z),
                    
                    Time.deltaTime * cameraManager.so_Camera.rotationOffSetSmooth);
            }
            
            cameraManager.cameraTransform.localRotation = Quaternion.Slerp(cameraManager.cameraTransform.localRotation, cameraManager.smoothOffset, 
                Time.deltaTime); 
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
                    Vector3.Lerp(transform.localPosition, transform.localPosition + new Vector3(posX,posY,posZ), cameraManager.so_Camera.positionOffSetSmooth);//cameraManager.so_Camera.positionOffSetSmooth * Time.deltaTime

            }
            else
            {
                transform.localRotation = 
                    Quaternion.Slerp(transform.localRotation, Quaternion.identity, cameraManager.so_Camera.rotationOffSetSmooth * Time.deltaTime);
                
                transform.localPosition = 
                    Vector3.Lerp(transform.localPosition, basePos, cameraManager.so_Camera.positionOffSetSmooth);
            }
            
            /*Quaternion shootingRot = transform.localRotation * Quaternion.Euler(-angleX, angleZ, 0);
            transform.localRotation = 
                Quaternion.Slerp(transform.localRotation, shootingRot, cameraManager.so_Camera.rotationOffSetSmooth * Time.deltaTime);
            
            float zRecoil = weapon.so_Weapon.weaponMode[(int)weapon.actualWeaponModeIndex].weaponRecoilPosZ;
            transform.localPosition = 
                Vector3.Lerp(transform.localPosition, transform.localPosition + new Vector3(0,0,-zRecoil), cameraManager.so_Camera.positionOffSetSmooth * Time.deltaTime);//cameraManager.so_Camera.positionOffSetSmooth * Time.deltaTime
*/
        }


        private float timeElaspedSlidingShake;
        private float shakePosX;
        private float shakePosY;
        private float shakePosz;

        private Vector3 GetShakingVector3()
        {
            var speed = PlayerController.Instance._rb.velocity.magnitude;
            return new Vector3(GetShakingSlidingX(), GetShakingSlidingY(), GetShakingSlidingZ()) * (speed / 50);
        }
        
        private float GetShakingSlidingX()
        {
            if(cameraManager.cameraSliding.timeElapsed <= 0) return 0f;
            
            timeElaspedSlidingShake += Time.deltaTime * 1;
            
            if ((shakeTimer < timeElaspedSlidingShake)) timeElaspedSlidingShake = 0;
            
            float posX = Random.Range(slidingShakePosX.x,
                slidingShakePosX.y);

            shakePosX = shakeTimer > timeElaspedSlidingShake / 2 ? 
                Mathf.Lerp(shakePosX, posX, Time.deltaTime * shakeTimerSpeed) : 
                Mathf.Lerp(0, shakePosX, Time.deltaTime);


            return shakePosX;
        }

        private float GetShakingSlidingY()
        {
            if(cameraManager.cameraSliding.timeElapsed <= 0) return 0f;
            
            float posY = Random.Range(slidingShakePosY.x,
                slidingShakePosY.y);
            
            shakePosY = shakeTimer > timeElaspedSlidingShake / 2 ? 
                Mathf.Lerp(shakePosY, posY, Time.deltaTime * shakeTimerSpeed) : 
                Mathf.Lerp(0, shakePosY, Time.deltaTime);

            return shakePosY;
        }
        
        private float GetShakingSlidingZ()
        {
            if(cameraManager.cameraSliding.timeElapsed <= 0) return 0f;
            
            float posZ = Random.Range(slidingShakePosZ.x,
                slidingShakePosZ.y);
                
            shakePosz = shakeTimer > timeElaspedSlidingShake / 2 ? 
                Mathf.Lerp(shakePosz, posZ, Time.deltaTime * shakeTimerSpeed) : 
                Mathf.Lerp(0, shakePosz, Time.deltaTime);

            return shakePosz;
        }


        private void AnimateWeaponReloading()
        {
            if(!weapon.isReloading) return;
            transform.localRotation = Quaternion.Slerp(transform.localRotation,
                    Quaternion.AngleAxis(reloadingRotX, Vector3.forward) * Quaternion.AngleAxis(reloadingRotY, Vector3.up),
                    cameraManager.so_Camera.weaponSwaySmooth * Time.deltaTime);
        }
    }
    
}
