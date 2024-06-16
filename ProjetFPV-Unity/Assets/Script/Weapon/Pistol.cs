using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;
using Weapon;
using Weapon.Interface;
using Random = UnityEngine.Random;

namespace Weapon
{
    public class Pistol : Shotgun
    {
        public override void InstantiateBulletImpact(RaycastHit hit)
        {
            base.InstantiateBulletImpact(hit);
            
            GameObject particle = Pooling.Instance.Pop("BulletImpact");
            particle.transform.position = hit.point;
            particle.transform.up = hit.normal;
            Pooling.Instance.DelayedDePop("BulletImpact", particle,5);

            if (so_Weapon.weaponMode[(int)actualWeaponModeIndex].doExplosion)
            {
                GameObject explosion = Pooling.Instance.Pop("ExplosionImpact");
                explosion.transform.position = hit.point;
            }
            
        }
    }
}

