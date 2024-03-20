using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Weapon;
using Object = UnityEngine.Object;

public class Barbatos : Shotgun
{
    private BarbatosInput barbatosInput;

    protected override void Start()
    {
        base.Start();
        barbatosInput = GetComponent<BarbatosInput>();
    }

    protected override void GetAllInput()
    {
        if (barbatosInput.isReceivingPrimary)
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
        
        /*if (barbatosInput.isReceivingSecondary)
        {
            if (actualWeaponModeIndex != WeaponMode.Secondary)
            {
                actualWeaponModeIndex = WeaponMode.Secondary;
                //WeaponRefreshement();
            }
            Shoot();
            Debug.Log("Secondary");
        }
        else
        {
            isShooting = false;
            canFire = true;
        }*/
        
            
        if (barbatosInput.isReceivingReload) Reload();
    }

    public void Secondary()
    {
        Debug.Log("Begin");
        if (actualWeaponModeIndex != WeaponMode.Secondary)
        {
            actualWeaponModeIndex = WeaponMode.Secondary;
            //WeaponRefreshement();
        }
        Shoot();
        Debug.Log("Secondary");
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
