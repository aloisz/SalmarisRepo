using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Weapon;

public class Barbatos : Shotgun
{
    private int standbyActualNumberOfBulletPrimaryMode, standbyActualNumberOfBulletSecondaryMode;
    
    
    protected override void GetAllInput()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (actualWeaponModeIndex != WeaponMode.Primary)
            {
                actualWeaponModeIndex = WeaponMode.Primary;
                //WeaponRefreshement();
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
                //WeaponRefreshement();
            }
            Shoot();
        }
        else
        {
            isShooting = false;
            canFire = true;
        }
            
        if (Input.GetKeyDown(KeyCode.R)) Reload();
    }
    
    
    public override void InstantiateBulletImpact(RaycastHit hit)
    {
        base.InstantiateBulletImpact(hit);
        GameObject particle =  Instantiate(GameManager.Instance.PS_BulletImpact, hit.point, Quaternion.identity, GameManager.Instance.transform);
        particle.transform.up = hit.normal;

        if (so_Weapon.weaponMode[(int)actualWeaponModeIndex].doExplosion)
        {
            GameObject explosion =  Instantiate(GameManager.Instance.explosion, hit.point, Quaternion.identity, GameManager.Instance.transform);
        }
    }
    
    
}
