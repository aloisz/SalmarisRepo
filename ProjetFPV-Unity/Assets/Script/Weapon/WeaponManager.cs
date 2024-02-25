using System;
using System.Collections;
using System.Collections.Generic;
using CameraBehavior;
using NaughtyAttributes;
using Player;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Weapon
{
    public class WeaponManager : MonoBehaviour
    {
        [Expandable]
        public SO_Weapon so_Weapon;
        
        protected bool canFire = true;
        protected bool isShooting;
        protected bool isReloading;
        
        protected float lastTimefired;
        protected int actualNumberOfBullet, standbyActualNumberOfBulletPrimaryMode, standbyActualNumberOfBulletSecondaryMode; // Permit to store the secondary mode number of bullet
        protected WeaponMode actualWeaponModeIndex;
        protected bool isChangingActualWeaponModeIndex;
        
        // Get All Component
        protected PlayerController PlayerController;
        protected Camera camera;
        
        protected virtual void Start()
        {
            PlayerController = PlayerController.Instance;
            camera = Camera.main;
            
            WeaponRefreshement();
        }
        
        /// <summary>
        /// when Changing Weapon/ this will adapt the value weapon to weapon
        /// </summary>
        protected virtual void WeaponRefreshement()
        {
            actualNumberOfBullet = so_Weapon.weaponMode[(int)actualWeaponModeIndex].numberOfBullet;
        }
       
        protected virtual void Update()
        {
            GetAllInput();
        }
        
        protected virtual void GetAllInput()
        {
            if (Input.GetKey(KeyCode.Mouse0)) Shoot();
            else
            {
                isShooting = false;
                canFire = true;
            }
            
            if (Input.GetKeyDown(KeyCode.R)) Reload();
            
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                isChangingActualWeaponModeIndex = !isChangingActualWeaponModeIndex;
                actualWeaponModeIndex = isChangingActualWeaponModeIndex ? WeaponMode.Secondary : WeaponMode.Primary;
                WeaponRefreshement();
            }
        }

        #region Shooting

        protected virtual void Shoot()
        {
            if (actualNumberOfBullet <= 0 || isReloading || !canFire) return; 
            switch (so_Weapon.weaponMode[(int)actualWeaponModeIndex].selectiveFireState)
            {
                case SelectiveFireType.Single:
                    SingleSelectiveFire();
                    break;
                
                case SelectiveFireType.Burst:
                    BurstSelectiveFire();
                    break;
                
                case SelectiveFireType.Auto:
                    AutoSelectiveFire();
                    break;
            }
        }

        /// <summary>
        /// Some Logic when shooting
        /// </summary>
        protected virtual void LogicWhenShooting()
        {
            isShooting = true;
            lastTimefired = Time.time;
            actualNumberOfBullet--;
            if(actualNumberOfBullet == 0) Reload();
        }

        /// <summary>
        /// Permit to Shoot in Single mode
        /// </summary>
        protected virtual void SingleSelectiveFire()
        {
            LogicWhenShooting();
            WichTypeMunitionIsGettingShot();
            canFire = false;
        }
        
        
        /// <summary>
        /// Permit to Shoot in Burst mode
        /// </summary>
        protected virtual void BurstSelectiveFire()
        {
            LogicWhenShooting();
        }
        
        /// <summary>
        /// Permit to Shoot in Automatic mode
        /// </summary>
        protected virtual void AutoSelectiveFire()
        {
            if (Time.time - lastTimefired > 1 / so_Weapon.weaponMode[(int)actualWeaponModeIndex].fireRate)
            {
                LogicWhenShooting();
                WichTypeMunitionIsGettingShot();
            }
        }

        /// <summary>
        /// Conduct the munition type to the correct path
        /// </summary>
        protected virtual void WichTypeMunitionIsGettingShot()
        {
            switch (so_Weapon.weaponMode[(int)actualWeaponModeIndex].munitionTypeState)
            {
                case MunitionType.Raycast:
                    Raycast();
                    break;
                case MunitionType.Projectile:
                    break;
            }
        }

        
        
        #region RayCast

        protected virtual void Raycast()
        {
            if (so_Weapon.weaponMode[(int)actualWeaponModeIndex].isHavingDispersion)
            {
                HitScanWithDispersion();
            }
            else HitScan();
        }
        protected virtual void HitScan()
        {
            RaycastHit hit;
            if (Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, 1000))
            {
                Debug.DrawRay(camera.transform.position, camera.transform.forward * 1000, Color.red, .2f);
                if (hit.transform.GetComponent<IDamage>() != null)
                {
                    hit.transform.GetComponent<IDamage>().Hit();
                }
                if (hit.transform.GetComponent<Collider>() != null)
                {
                    InstantiateBulletImpact(hit);
                }
            }
        }
        protected virtual void HitScanWithDispersion()
        {
            RaycastHit hit;
            int howManyRay = Random.Range(so_Weapon.weaponMode[(int)actualWeaponModeIndex].howManyBulletShot.x,
                so_Weapon.weaponMode[(int)actualWeaponModeIndex].howManyBulletShot.y);
            for (int i = 0; i < howManyRay; i++)
            {
                float zAxisDispersion = Random.Range(so_Weapon.weaponMode[(int)actualWeaponModeIndex].zAxisDispersion.x,
                    so_Weapon.weaponMode[(int)actualWeaponModeIndex].zAxisDispersion.y);
                
                float yAxisDispersion = Random.Range(so_Weapon.weaponMode[(int)actualWeaponModeIndex].yAxisDispersion.x,
                    so_Weapon.weaponMode[(int)actualWeaponModeIndex].yAxisDispersion.y);

                Vector3 direction = camera.transform.forward + new Vector3(0, yAxisDispersion, zAxisDispersion);
                
                if (Physics.Raycast(camera.transform.position, direction, out hit, 1000))
                {
                    Debug.DrawRay(camera.transform.position, direction * 1000, Color.red, .2f);
                    if (hit.transform.GetComponent<IDamage>() != null)
                    {
                        hit.transform.GetComponent<IDamage>().Hit();
                    }

                    if (hit.transform.GetComponent<Collider>() != null)
                    {
                        InstantiateBulletImpact(hit);
                    }
                    
                }
            }
        }

        protected virtual void InstantiateBulletImpact(RaycastHit hit)
        {
            GameObject particle =  Instantiate(GameManager.Instance.PS_BulletImpact, hit.point, Quaternion.identity);
            particle.transform.up = hit.normal;
        }

        #endregion
        
        
        #endregion
        
        //-------------------------------------------
        #region Reload

        protected virtual void Reload()
        {
            isReloading = true;
            StartCoroutine(TimeToReload());
        }

        private IEnumerator TimeToReload()
        {
            yield return new WaitForSeconds(so_Weapon.weaponMode[(int)actualWeaponModeIndex].timeToReload);
            actualNumberOfBullet = so_Weapon.weaponMode[(int)actualWeaponModeIndex].numberOfBullet;
            isReloading = false;
        }

        #endregion
        
        //-------------------------------------------
        #region Debug

        private void OnGUI()
        {
            // Set up GUI style for the text
            GUIStyle style = new GUIStyle();
            style.fontSize = 24;
            style.normal.textColor = Color.green;
            
            // Set the position and size of the text
            Rect rect = new Rect(1500, 10, 200, 50);
            Rect rect1 = new Rect(1500, 60, 200, 50);
            Rect rect2 = new Rect(1500, 110, 200, 50);
            Rect rect3 = new Rect(1500, 160, 200, 50);
            Rect rect4 = new Rect(1500, 210, 200, 50);

            // Display the text on the screen
            GUI.Label(rect, $"numberOfBullet : {so_Weapon.weaponMode[(int)actualWeaponModeIndex].numberOfBullet}", style);
            GUI.Label(rect1, $"actualNumberOfBullet : {actualNumberOfBullet}", style);
            GUI.Label(rect2, $"timeToReload : {so_Weapon.weaponMode[(int)actualWeaponModeIndex].timeToReload}", style);
            GUI.Label(rect3, $"fireRate : {so_Weapon.weaponMode[(int)actualWeaponModeIndex].fireRate}", style);
            GUI.Label(rect4, $"weaponModeIndex : {actualWeaponModeIndex}", style);
        }        

        #endregion
        
        //-------------------------------------------
    }
}


