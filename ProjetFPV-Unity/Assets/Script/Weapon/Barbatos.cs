using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Weapon;
using Weapon.Interface;
using Object = UnityEngine.Object;

public class Barbatos : Shotgun
{
    private BarbatosInput barbatosInput;
    
    [Header("Projectile")] 
    [SerializeField] private LayerMask whoIsTheTarget;
    [SerializeField] private float dragApply;
    [SerializeField] private float gravityApplied;

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
            }
            isFirstBulletGone = false;
            ShootingAction();
        }
        else
        {
            isShooting = false;
            canFire = true;
        }

        if (barbatosInput.isReceivingSecondary) Secondary();
        
        if (barbatosInput.isReceivingReload) Reload();
    }

    public void Secondary()
    {
        if (actualWeaponModeIndex != WeaponMode.Secondary)
        {
            actualWeaponModeIndex = WeaponMode.Secondary;
        }
        ShootingAction();
    }


    protected override void HitScanLogic(RaycastHit hit)
    {
        base.HitScanLogic(hit);
        if(isFirstBulletGone) return;
        if (hit.transform.TryGetComponent<IExplosion>(out IExplosion explosion))
        {
            explosion.HitScanExplosion(whoIsTheTarget);
        }
        isFirstBulletGone = true;
    }
    
    
    internal BarbatwoBullet bulletProjectile;
    protected override void ShootProjectile()
    {
        base.ShootProjectile();
        bulletProjectile = bulletProjectileGO.GetComponent<BarbatwoBullet>();
        
        // Logic
        bulletProjectile.EnableMovement(true);  
        bulletProjectile.transform.rotation *= Quaternion.AngleAxis(90, PlayerController.transform.right);
        bulletProjectile.SetTheBulletDir(GetTheAimDirection());
        bulletProjectile.UseGravity(true);
        bulletProjectile.GravityApplied(gravityApplied);
        bulletProjectile.AddVelocity(so_Weapon.weaponMode[(int)actualWeaponModeIndex].bulletSpeed + PlayerController.direction.magnitude);
        bulletProjectile.AddDamage(so_Weapon.weaponMode[(int)actualWeaponModeIndex].bulletDamage);
        bulletProjectile.PoolingKeyName(so_Weapon.weaponMode[(int)actualWeaponModeIndex].poolingPopKey);
        
        bulletProjectile.DragModification(dragApply);
        bulletProjectile.RocketJumpForceApplied(so_Weapon.weaponMode[(int)actualWeaponModeIndex].rocketJumpForceApplied);
        bulletProjectile.WhoIsTheTarget(whoIsTheTarget);
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
