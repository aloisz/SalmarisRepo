using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using Weapon;
using Weapon.Interface;
using Object = UnityEngine.Object;

public class Barbatos : Shotgun
{
    private BarbatosInput barbatosInput;
    [SerializeField] protected List<Transform> vfxPos;
    
    private float lastTimeFired_0; // primary
    private float lastTimeFired_1; // secondary
    private bool isPrimary;

    protected override void Start()
    {
        base.Start();
        barbatosInput = GetComponent<BarbatosInput>();
        WeaponState.Instance.barbatos.OnHudShoot += UpdateLastTimeFired;
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
            lastTimefired = lastTimeFired_0;
            isPrimary = true;
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
        lastTimefired = lastTimeFired_1;
        isPrimary = false;
        ShootingAction();
    }
    


    protected override void HitScanLogic(RaycastHit hit)
    {
        base.HitScanLogic(hit);
        if(isFirstBulletGone) return;
        if (hit.transform.TryGetComponent<IExplosion>(out IExplosion explosion))
        {
            isFirstBulletGone = true;
            explosion.HitScanExplosion(so_Weapon.weaponMode[(int)actualWeaponModeIndex].whoIsTheTarget);
        }
    }

    private void UpdateLastTimeFired()
    {
        if (isPrimary)
        {
            lastTimeFired_0 = lastTimefired;
        }
        else
        {
            lastTimeFired_1 = lastTimefired;
        }

        PlayParticle();
    }

    public override void Reload()
    {
        base.Reload();
        ParticleSystem particle = Instantiate(so_Weapon.weaponMode[(int)actualWeaponModeIndex].reloadParticle,
            vfxPos[2].position, gunBarrelPos.transform.rotation, transform);
        particle.Play();
        
        
    }

    private void PlayParticle()
    {
        if(so_Weapon.weaponMode[(int)actualWeaponModeIndex].weaponParticle == null) return;

        if (isPrimary)
        {
            ParticleSystem particle = Instantiate(so_Weapon.weaponMode[(int)actualWeaponModeIndex].weaponParticle,
                vfxPos[0].position, gunBarrelPos.transform.rotation, transform);
            particle.Play();
        }
        else
        {
            ParticleSystem particle = Instantiate(so_Weapon.weaponMode[(int)actualWeaponModeIndex].weaponParticle,
                vfxPos[1].position, gunBarrelPos.transform.rotation, transform);
            particle.Play();
        }
    }
    
    
    internal BarbatwoBullet bulletProjectile;
    protected override void ShootProjectile()
    {
        base.ShootProjectile();
        bulletProjectile = bulletProjectileGO.GetComponent<BarbatwoBullet>();
        
        // Logic
        bulletProjectile.EnableMovement(true);  
        bulletProjectile.transform.rotation *= Quaternion.AngleAxis(90, PlayerController.transform.right);
        bulletProjectile.UseGravity(true);
        bulletProjectile.GravityApplied(so_Weapon.weaponMode[(int)actualWeaponModeIndex].gravityApplied);
        bulletProjectile.AddVelocity(so_Weapon.weaponMode[(int)actualWeaponModeIndex].bulletSpeed + PlayerController.direction.magnitude);
        bulletProjectile.AddDamage(so_Weapon.weaponMode[(int)actualWeaponModeIndex].bulletDamage);
        bulletProjectile.PoolingKeyName(so_Weapon.weaponMode[(int)actualWeaponModeIndex].poolingPopKey);
        
        bulletProjectile.DragModification(so_Weapon.weaponMode[(int)actualWeaponModeIndex].dragApply);
        bulletProjectile.RocketJumpForceApplied(so_Weapon.weaponMode[(int)actualWeaponModeIndex].rocketJumpForceApplied);
        bulletProjectile.WhoIsTheTarget(so_Weapon.weaponMode[(int)actualWeaponModeIndex].whoIsTheTarget);

        switch (so_Weapon.weaponMode[(int)actualWeaponModeIndex].projectileType)
        {
            case ProjectileType.Simple:
                bulletProjectile.DoBounce(ProjectileType.Simple, 0);
                break;
            case ProjectileType.Bounce:
                bulletProjectile.DoBounce(ProjectileType.Bounce, so_Weapon.weaponMode[(int)actualWeaponModeIndex].bounceNbr);
                break;
        }
        
        bulletProjectile.SetTheBulletDir(Vector3.zero);
        bulletProjectile.SetTheBulletDir(GetTheAimDirection());
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
        
        DecalSpawnerManager.Instance.SpawnDecal(hit.point, hit.normal, DecalSpawnerManager.possibleDecals.shotgunImpact);
        
        if (so_Weapon.weaponMode[(int)actualWeaponModeIndex].doExplosion)
        {
            GameObject explosion = Pooling.instance.Pop("ExplosionImpact");
            explosion.transform.position = hit.point;
        }
    }
}

