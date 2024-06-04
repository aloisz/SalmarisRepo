using System.Collections;
using System.Collections.Generic;
using AI;
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
        private AI_TrashMob_Shooter trashMobShooter;
        protected override void Start()
        {
            base.Start();
            actualWeaponModeIndex = WeaponMode.Primary;
            player = Player.PlayerController.Instance;
            trashMobShooter = GetComponent<AI_TrashMob_Shooter>();
        }

        private Vector3 playerHitPos;
        protected override void ShootProjectile()
        {
            base.ShootProjectile();
            bulletProjectile = bulletProjectileGO.GetComponent<BulletBehavior>();
        
            // Logic
            playerPos = player.transform.position;

            RaycastHit hit;
            if (Physics.Raycast(playerPos, -player.transform.up, out hit, 1000))
            {
                playerHitPos = hit.point;
            }
            float distToPlayer = Vector3.Distance(playerPos, transform.position);
            float time = timeToArrive.Evaluate(distToPlayer);
            
            bulletProjectile.transform.rotation *= Quaternion.AngleAxis(90, PlayerController.transform.right);
            bulletProjectile.EnableMovement(true);
            bulletProjectile.UseGravity(true);
            
            bulletProjectile.transform.DOJump(playerHitPos, throwPower, 1, time).SetEase(Ease.Linear).OnComplete((() => 
                bulletProjectile.UseGravity(true)));
            
            bulletProjectile.AddDamage(so_Weapon.weaponMode[(int)actualWeaponModeIndex].bulletDamage);
            bulletProjectile.PoolingKeyName(so_Weapon.weaponMode[(int)actualWeaponModeIndex].poolingPopKey);
            
            
            //Animator
            if(trashMobShooter.isPawnDead) return;
            trashMobShooter.animatorTrashShooter.ChangeState(trashMobShooter.animatorTrashShooter.ATTACK, .3f);
        }
        
    }
}

