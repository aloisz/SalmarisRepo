using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Weapon.Interface;

public class HeavyArtillery : ShootingLogicModule
{
    protected override void HitScanLogic(RaycastHit hit)
    {
        base.HitScanLogic(hit);
        
        // Create an explosion when raycast touch Bullet with IExplosion interface 
        if (hit.transform.GetComponent<IExplosion>() != null)
        {
            hit.transform.GetComponent<IExplosion>().Explosion();
        }
    }
}
