using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Weapon.EnemyWeapon
{
    public class Laser : ShootingLogicModule
    {
        [SerializeField] protected float timeBeforeShooting;
        protected float timeElaspsed = 0f;
        protected bool canShoot;
        protected Vector3 aimDir;

        protected override void Update()
        {
            base.Update();
            if(canShoot) return;
            timeElaspsed += Time.deltaTime * 1;
            if (timeElaspsed > timeBeforeShooting)
            {
                canShoot = true;
                timeElaspsed = 0;
                aimDir =  ((Player.PlayerController.Instance.transform.position + Vector3.up) - gunBarrelPos.transform.position).normalized;
            }
        }
        
        public override void SphereCastSingleHitScan(float maxDistance, float radius)
        {
            if(!canShoot) return;
            canShoot = false;
            RaycastHit hit;
            if(Physics.SphereCast(gunBarrelPos.position, radius, aimDir, out hit, maxDistance, so_Weapon.hitLayer))
            {
                Debug.DrawRay(gunBarrelPos.position, aimDir * maxDistance, Color.red, .2f);
                InitialiseLineRenderer(hit);
                HitScanLogic(hit);
            }
        }
    }
}

