using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEngine;
using Weapon;


namespace Weapon.EnemyWeapon
{
    public class SMG : ShootingLogicModule
    {
        internal BulletBehavior bulletProjectile;
        private AI_TrashMob_Shooter trashMobShooter;
        protected override void Start()
        {
            base.Start();
            actualWeaponModeIndex = WeaponMode.Primary;
            trashMobShooter = GetComponent<AI_TrashMob_Shooter>();
        }
        
        protected override void ShootProjectile()
        {
            base.ShootProjectile();
            bulletProjectile = bulletProjectileGO.GetComponent<BulletBehavior>();
        
            // Logic
            bulletProjectile.EnableMovement(true);  
            bulletProjectile.transform.rotation *= Quaternion.AngleAxis(90, PlayerController.transform.right);
            bulletProjectile.SetTheBulletDir(GetThePlayerDirection());
            bulletProjectile.AddVelocity(so_Weapon.weaponMode[(int)actualWeaponModeIndex].bulletSpeed);
            bulletProjectile.AddDamage(so_Weapon.weaponMode[(int)actualWeaponModeIndex].bulletDamage);
            bulletProjectile.PoolingKeyName(so_Weapon.weaponMode[(int)actualWeaponModeIndex].poolingPopKey);
            
            //Animator
            if(trashMobShooter.isPawnDead) return;
            trashMobShooter.animatorTrashShooter.ChangeState(trashMobShooter.animatorTrashShooter.ATTACK, .3f);
        }

        private Vector3 GetThePlayerDirection()
        {
            Vector3 aimDir = ((Player.PlayerController.Instance.transform.position + new Vector3(0,.5f,0) ) - gunBarrelPos.transform.position).normalized;
            return aimDir;
        }
    }
}