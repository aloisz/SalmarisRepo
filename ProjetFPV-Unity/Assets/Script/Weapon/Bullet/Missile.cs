using System.Collections;
using System.Collections.Generic;
using Player;
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
        if (!EnableMovement(bullet.isMoving)) return;
        Vector3 bulletDir = PlayerController.Instance.transform.position - transform.position;
        rb.velocity = (bulletDir) * (bullet.speed * Time.fixedDeltaTime);
        rb.isKinematic = false;
    }

    private void TrackPlayer()
    {
        
    }
}
