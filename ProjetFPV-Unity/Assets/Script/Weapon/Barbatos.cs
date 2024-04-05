using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Weapon;
using Object = UnityEngine.Object;

public class Barbatos : Shotgun
{
    private BarbatosInput barbatosInput;
    
    [Header("Charging Weapon")]
    [SerializeField] private float chargingMaxValue;
    [SerializeField] private float chargingMultiplier;
    private float chargingActualValue;

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
        if (barbatosInput.isReceivingSecondary) Secondary();
        else chargingActualValue = 0;
        
            
        if (barbatosInput.isReceivingReload) Reload();
    }

    public void Secondary()
    {
        if (actualWeaponModeIndex != WeaponMode.Secondary)
        {
            actualWeaponModeIndex = WeaponMode.Secondary;
            //WeaponRefreshement();
        }
        
        if(!IsSecondaryCharged()) return;
        isFirstBulletGone = false;
        Shoot();
    }

    public bool IsSecondaryCharged()
    {
        bool isCharged = false;
        if (chargingActualValue < chargingMaxValue)
        {
            chargingActualValue += Time.deltaTime * chargingMultiplier;
            if (chargingActualValue >= chargingMaxValue)
            {
                isCharged =  true;
            }
        }
        return isCharged;
    }


    public override void SwapWeapon()
    {
        if (actualNumberOfBullet <= 0)
        {
            Reload();
        }
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
