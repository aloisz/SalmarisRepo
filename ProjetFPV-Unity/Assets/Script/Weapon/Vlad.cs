using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Weapon;
using DG.Tweening;

public class Vlad : HeavyArtillery
{
    private VladInput vladInput;
    private bool hasClicked;

    private Transform baseTransform;

    [Header("Overheating")]
    public float vladOverheatActualValue;
    [SerializeField] private float vladOverheatMultiplier;
    [SerializeField] private float vladCoolingMultiplier;
    public float vladOverheatMin;
    public float vladOverheatMax;
    [SerializeField] private bool isVladOnFire;
    private bool canNotShoot;
    
    [Space] 
    [Header("Draw Speed")] 
    [SerializeField] private float bulletSpeedMultiplier;
    [SerializeField] private  float DrawSpeedMultiplier;
    [SerializeField] private  float DrawSpeedMax;
    private float basebulletSpeedMultiplier;
    
    [Space] 
    [Header("Bullet Damage multiplier")] 
    [SerializeField] private float bulletDamageMultiplier = 1;
    [SerializeField] private  float DrawDamageMultiplier;
    [SerializeField] private  float DrawDamageMax;
    [SerializeField] private  float DrawDamageMultiplierOverheating;
    private float basebulletDamageMultiplier;

    protected override void Start()
    {
        base.Start();
        vladInput = GetComponent<VladInput>();
        baseTransform = transform;
        basebulletSpeedMultiplier = bulletSpeedMultiplier;
        basebulletDamageMultiplier = bulletDamageMultiplier;
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
            if(vladOverheatActualValue <= vladOverheatMin)
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
                if (vladOverheatActualValue >= vladOverheatMin)
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
                if (vladOverheatActualValue <= vladOverheatMin)
                {
                    isVladOnFire = false;
                }
            }
        }
    }
    

    private void DrawSpikes()
    {
        if (bulletSpeedMultiplier <= DrawSpeedMax)
        {
            bulletSpeedMultiplier += Time.deltaTime * DrawSpeedMultiplier;
        }
        if (bulletDamageMultiplier <= DrawDamageMax)
        {
            bulletDamageMultiplier += Time.deltaTime * DrawDamageMultiplier;
        }
    }

    private void LooseSpikes()
    {
        if (hasClicked && !vladInput.isReceivingSecondary)
        {
            Shoot();
            bulletSpeedMultiplier = basebulletSpeedMultiplier;
            bulletDamageMultiplier = basebulletDamageMultiplier;
            hasClicked = false;
        }
    }

    protected override void ShootProjectile()
    {
        base.ShootProjectile();
        bulletProjectile.AddVelocity(so_Weapon.weaponMode[(int)actualWeaponModeIndex].bulletSpeed * bulletSpeedMultiplier);
        if (isVladOnFire)
        {
            bulletProjectile.IsBulletOnFire(isVladOnFire);
            bulletProjectile.AddDamage(so_Weapon.weaponMode[(int)actualWeaponModeIndex].bulletDamage * DrawDamageMultiplierOverheating);
        }
        else
        {
            bulletProjectile.AddDamage(so_Weapon.weaponMode[(int)actualWeaponModeIndex].bulletDamage * bulletDamageMultiplier);
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
