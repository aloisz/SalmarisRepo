using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Weapon;

namespace Weapon
{
    public class Pistol : WeaponManager
    {
        protected override void InstantiateBulletImpact(RaycastHit hit)
        {
            base.InstantiateBulletImpact(hit);
            GameObject particle =  Instantiate(GameManager.Instance.PS_BulletImpact, hit.point, Quaternion.identity, GameManager.Instance.transform);
            particle.transform.up = hit.normal;
        }
    }
}

