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
    public class WeaponManager : MonoBehaviour, IRaycast
    { 
        [Expandable]
        public SO_Weapon so_Weapon;
        
        protected bool canFire = true;
        protected bool isShooting;
        protected bool isReloading;
        
        protected float lastTimefired;
        protected int actualNumberOfBullet, standbyActualNumberOfBulletPrimaryMode, standbyActualNumberOfBulletSecondaryMode; // Permit to store the secondary mode number of bullet
        public WeaponMode actualWeaponModeIndex;
        protected bool isChangingActualWeaponModeIndex;
        
        // Get All Component
        protected PlayerController PlayerController;
        public Camera camera;
        private RaycastModule raycastModule;
        
        protected virtual void Start()
        {
            PlayerController = PlayerController.Instance;
            camera = Camera.main;
            raycastModule = GetComponent<RaycastModule>();
            
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
            if (Input.GetKey(KeyCode.Mouse0))
            {
                if (actualWeaponModeIndex != WeaponMode.Primary)
                {
                    actualWeaponModeIndex = WeaponMode.Primary;
                    WeaponRefreshement();
                }
                Shoot();
            }
            else
            {
                isShooting = false;
                canFire = true;
            }
            
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                if (actualWeaponModeIndex != WeaponMode.Secondary)
                {
                    actualWeaponModeIndex = WeaponMode.Secondary;
                    WeaponRefreshement();
                }
                Shoot();
                /*isChangingActualWeaponModeIndex = !isChangingActualWeaponModeIndex;
                actualWeaponModeIndex = isChangingActualWeaponModeIndex ? WeaponMode.Secondary : WeaponMode.Primary;
                WeaponRefreshement();*/
            }
            else
            {
                isShooting = false;
                canFire = true;
            }
            
            if (Input.GetKeyDown(KeyCode.R)) Reload();
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

        
        
        protected virtual void Raycast()
        {
            raycastModule.ChooseEnum(so_Weapon.weaponMode[(int)actualWeaponModeIndex].raycastType);
        }
        
        
        #region FeedBack

        public virtual void InstantiateBulletImpact(RaycastHit hit)
        {
            
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
        public virtual RaycastType ChooseRaycastType(RaycastType raycastType)
        {
            return raycastType;
        }
    }
}


