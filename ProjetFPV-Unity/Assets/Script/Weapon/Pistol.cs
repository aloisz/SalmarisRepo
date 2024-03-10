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
    public class Pistol : RaycastModule
    {
        protected override void InstantiateBulletImpact(RaycastHit hit)
        {
            base.InstantiateBulletImpact(hit);
            GameObject particle =  Instantiate(GameManager.Instance.PS_BulletImpact, hit.point, Quaternion.identity, GameManager.Instance.transform);
            particle.transform.up = hit.normal;
        }
    }
}

