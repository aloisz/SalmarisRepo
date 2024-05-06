using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Player;
using UnityEngine;


namespace Weapon.EnemyWeapon
{
    public class MissileCurve : ShootingLogicModule
    {
        internal BulletBehavior bulletProjectile;
        [SerializeField] protected AnimationCurve timeToArrive;
        [SerializeField] protected float throwPower;

        private PlayerController player;
        private Vector3 playerPos;
        protected override void Start()
        {
            base.Start();
            actualWeaponModeIndex = WeaponMode.Primary;
            player = Player.PlayerController.Instance;
            
        }
        
        protected override void ShootProjectile()
        {
            base.ShootProjectile();
            bulletProjectile = bulletProjectileGO.GetComponent<BulletBehavior>();
        
            // Logic
            playerPos = player.transform.position;
            float distToPlayer = Vector3.Distance(playerPos, transform.position);
            float time = timeToArrive.Evaluate(distToPlayer);
            
            bulletProjectile.transform.rotation *= Quaternion.AngleAxis(90, PlayerController.transform.right);
            bulletProjectile.EnableMovement(true);
            bulletProjectile.transform.DOJump(playerPos, throwPower, 1, time);
            bulletProjectile.AddDamage(so_Weapon.weaponMode[(int)actualWeaponModeIndex].bulletDamage);
            bulletProjectile.PoolingKeyName(so_Weapon.weaponMode[(int)actualWeaponModeIndex].poolingPopKey);
        }
        
    }
}

