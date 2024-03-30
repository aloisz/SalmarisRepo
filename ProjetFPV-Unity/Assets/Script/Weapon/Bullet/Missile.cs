using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;
using Weapon.Interface;

public class Missile : BulletBehavior,IExplosion
{

    protected virtual void OnDisable()
    {
        base.OnDisable();
    }
    
    // Here put following logic when bullet collide with walkableMask
    protected override void CollideWithWalkableMask(Collision collision)
    {
        Explosion();
        lineRenderer.enabled = false;
        Pooling.instance.DePop(bullet.PoolingKeyName, gameObject);
    }
    
    // Here put following logic when bullet collide with enemyMask
    protected override void CollideWithEnemyMask(Collision collision)
    {
        Explosion();
        lineRenderer.enabled = false;
        collision.transform.GetComponent<IDamage>().Hit(bullet.damage);
        Pooling.instance.DePop(bullet.PoolingKeyName, gameObject);
    }
    
    protected override void FixedUpdate()
    {
        if (!EnableMovement(bullet.isMoving)) return;
        TrackPlayer();
    }

    private void TrackPlayer()
    {
        Vector3 bulletDir = (PlayerController.Instance.transform.position - transform.position).normalized;
        
        transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation (bulletDir), Time.fixedDeltaTime * 40f);
        rb.velocity = (bulletDir) * (bullet.speed * Time.fixedDeltaTime);
        rb.isKinematic = false;
    }

    public void Explosion()
    {
        GameObject Explosion = Pooling.instance.Pop("Explosion");
        Explosion.transform.position = transform.position;
        Explosion.transform.rotation = Quaternion.identity;
    }
}
