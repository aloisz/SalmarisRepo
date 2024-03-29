using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Weapon;

public class MissileLauncher : HeavyArtillery
{
    internal Missile bulletProjectile;
    protected override void Start()
    {
        base.Start();
        actualWeaponModeIndex = WeaponMode.Primary;
        //StartCoroutine(Logic());
    }

    private IEnumerator Logic()
    {
        Shoot();
        yield return null;
        StartCoroutine(Logic());
    }
    
    protected override void ShootProjectile()
    {
        base.ShootProjectile();
        bulletProjectile = bulletProjectileGO.GetComponent<Missile>();
        
        // Logic
        bulletProjectile.EnableMovement(true);  
        bulletProjectile.transform.rotation *= Quaternion.AngleAxis(90, PlayerController.transform.right);
        bulletProjectile.GetTheBulletDir(GetThePlayerDirection());
        bulletProjectile.AddVelocity(so_Weapon.weaponMode[(int)actualWeaponModeIndex].bulletSpeed);
        bulletProjectile.AddDamage(so_Weapon.weaponMode[(int)actualWeaponModeIndex].bulletDamage);
    }

    private Vector3 GetThePlayerDirection()
    {
        Vector3 aimDir = (Player.PlayerController.Instance.transform.position - gunBarrelPos.transform.position).normalized;
        return aimDir;
    }
}
