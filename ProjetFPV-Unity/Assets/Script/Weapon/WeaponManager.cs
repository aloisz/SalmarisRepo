using System;
using System.Collections;
using System.Collections.Generic;
using CameraBehavior;
using NaughtyAttributes;
using Player;
using UnityEngine;

namespace Weapon
{
    public class WeaponManager : MonoBehaviour
    {
        [Expandable]
        public SO_WeaponManager so_Weapon;
        public SelectFire selectFire;
        
        protected bool canFire = true;
        protected bool isShooting;
        protected bool isReloading;
        
        protected float lastTimefired;
        protected int actualNumberOfBullet;
        
        
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
        /// when Changing Weapon this will adapt the value weapon to weapon
        /// </summary>
        protected virtual void WeaponRefreshement()
        {
            actualNumberOfBullet = so_Weapon.modeOfWeapon[(int)selectFire].numberOfBullet;
        }
        
        protected virtual void Update()
        {
            GetAllInput();
            if (actualNumberOfBullet == 0) Reload();
        }

        protected virtual void GetAllInput()
        {
            if (Input.GetKey(KeyCode.Mouse0))
            {
                Shoot();
            }
            else
            {
                isShooting = false;
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                Reload();
            }
        }
        
        
        protected virtual void Shoot()
        {
            if (Time.time - lastTimefired > 1 / so_Weapon.modeOfWeapon[(int)selectFire].fireRate && !isReloading && canFire)
            {
                if (actualNumberOfBullet > 0) HitScan();
            }
        }

        private void HitScan()
        {
            isShooting = true;
            lastTimefired = Time.time;
            actualNumberOfBullet--;
            RaycastHit hit;
            if (Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, 1000))
            {
                Debug.DrawRay(camera.transform.position, camera.transform.forward * 1000, Color.red, .2f);
                if (hit.transform.GetComponent<IDamage>() != null)
                {
                    hit.transform.GetComponent<IDamage>().Hit();
                }
            }
        }
        
        protected virtual void Reload()
        {
            isReloading = true;
            StartCoroutine(TimeToReload());
        }

        private IEnumerator TimeToReload()
        {
            yield return new WaitForSeconds(so_Weapon.modeOfWeapon[(int)selectFire].timeToReload);
            actualNumberOfBullet = so_Weapon.modeOfWeapon[(int)selectFire].numberOfBullet;
            isReloading = false;
        }



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

            // Display the text on the screen
            GUI.Label(rect, $"numberOfBullet : {so_Weapon.modeOfWeapon[0].numberOfBullet}", style);
            GUI.Label(rect1, $"actualNumberOfBullet : {actualNumberOfBullet}", style);
            GUI.Label(rect2, $"timeToReload : {so_Weapon.modeOfWeapon[0].timeToReload}", style);
            GUI.Label(rect3, $"fireRate : {so_Weapon.modeOfWeapon[0].fireRate}", style);
        }        

        #endregion
    }
}


public enum SelectFire
{
    FirstMode,
    SecondMode
}


