using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Weapon;

public class Vlad : HeavyArtillery
{
    private VladInput vladInput;
    private bool hasClicked;

    [Header("Overheating")]
    [SerializeField] private float vladOverheatActualValue;
    [SerializeField] private float vladOverheatMultiplier;
    [SerializeField] private float vladCoolingMultiplier;
    [SerializeField] private float vladOverheat;
    [SerializeField] private float vladOverheatMax;
    [SerializeField] private bool isVladOnFire;
    private bool canNotShoot;
    [Space]
    [Header("Spikes")]
    [SerializeField] private float bulletSpeedMultiplier;
    [SerializeField] private  float spikesMultiplier;
    [SerializeField] private  float spikesMax;

    protected override void Start()
    {
        base.Start();
        vladInput = GetComponent<VladInput>();
    }

    protected override void GetAllInput()
    {
        Primary();
        Secondary();
        Overheating();
        LooseSpikes();
            
        if (vladInput.isReceivingReload) Reload();
    }

    private void Primary()
    {
        if (vladInput.isReceivingPrimary)
        {
            if (actualWeaponModeIndex != WeaponMode.Primary)
            {
                actualWeaponModeIndex = WeaponMode.Primary;
                WeaponRefreshement();
            }
            
            if(vladOverheatActualValue >= vladOverheatMax)
            {
                canNotShoot = true;
            }
            if(vladOverheatActualValue <= vladOverheat)
            {
                canNotShoot = false;
            }
            
            if(canNotShoot) return;
            Shoot();
        }
        else
        {
            isShooting = false;
            canFire = true;
        }
    }

    private void Secondary()
    {
        if (vladInput.isReceivingSecondary && canFire)
        {
            if (actualWeaponModeIndex != WeaponMode.Secondary)
            {
                actualWeaponModeIndex = WeaponMode.Secondary;
                WeaponRefreshement();
            }
            DrawSpikes();
            hasClicked = true;
        }
        else
        {
            isShooting = false;
            canFire = true;
        }
    }

    private void Overheating()
    {
        if (vladInput.isReceivingPrimary && !canNotShoot)
        {
            if (vladOverheatActualValue <= vladOverheatMax)
            {
                vladOverheatActualValue += Time.deltaTime * vladOverheatMultiplier;
                if (vladOverheatActualValue >= vladOverheat)
                {
                    isVladOnFire = true;
                }
            }
        }
        else
        {
            if (vladOverheatActualValue >= 0)
            {
                vladOverheatActualValue -= Time.deltaTime * vladCoolingMultiplier;
                if (vladOverheatActualValue <= vladOverheat)
                {
                    isVladOnFire = false;
                }
            }
        }
    }
    

    private void DrawSpikes()
    {
        if (bulletSpeedMultiplier <= spikesMax) bulletSpeedMultiplier += Time.deltaTime * spikesMultiplier;
    }

    private void LooseSpikes()
    {
        if (hasClicked && !vladInput.isReceivingSecondary)
        {
            Shoot();
            hasClicked = false;
            bulletSpeedMultiplier = 0;  
        }
    }

    protected override void ShootProjectile()
    {
        base.ShootProjectile();
        bulletProjectile.AddVelocity(so_Weapon.weaponMode[(int)actualWeaponModeIndex].bulletSpeed * bulletSpeedMultiplier);
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
