using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Weapon;
using Object = UnityEngine.Object;

public class Barbatos : Shotgun
{
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
        
        GameObject particle = Pooling.instance.Pop("BulletImpact");
        particle.transform.position = hit.point;
        particle.transform.up = hit.normal;
        Pooling.instance.DelayedDePop("BulletImpact", particle,3);
        
        if (so_Weapon.weaponMode[(int)actualWeaponModeIndex].doExplosion)
        {
            GameObject explosion = Pooling.instance.Pop("ExplosionImpact");
            explosion.transform.position = hit.point;
        }
    }
    
    
}
