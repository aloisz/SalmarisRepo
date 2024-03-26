using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : BulletBehavior
{

    protected override void Start()
    {
        base.Start();
        
    }
    
    // Here put following logic when bullet collide with walkableMask
    protected override void CollideWithWalkableMask(Collision collision)
    {
        
    }
    
    // Here put following logic when bullet collide with enemyMask
    protected override void CollideWithEnemyMask(Collision collision)
    {
        
    }
    
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    private void TrackPlayer()
    {
        
    }
}
