using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;

public class BarbatwoBullet : BulletBehavior
{
    protected float drag;
    protected virtual void OnDisable()
    {
        base.OnDisable();
        bulletShot = false;
    }
    
    // Here put following logic when bullet collide with walkableMask
    protected override void CollideWithWalkableMask(Collision collision)
    {
        Explosion();
        trailRenderer.enabled = false;
        Pooling.instance.DePop(bullet.PoolingKeyName, gameObject);
    }
    
    // Here put following logic when bullet collide with enemyMask
    protected override void CollideWithEnemyMask(Collision collision)
    {
        Explosion();
        trailRenderer.enabled = false;
        collision.transform.GetComponent<IDamage>().Hit(bullet.damage);
        Pooling.instance.DePop(bullet.PoolingKeyName, gameObject);
    }
    
    protected override void FixedUpdate()
    {
        if (!EnableMovement(bullet.isMoving)) return;
        if(rb.useGravity) rb.AddForce(Vector3.down * bullet.gravityApplied); 
        ShootBullet();
    }

    private bool bulletShot = false;
    private void ShootBullet()
    {
        if(bulletShot) return;
        bulletShot = true;
        
        rb.isKinematic = false;
        rb.drag = drag;
        transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation (GetTheBulletDir(bullet.bulletDir)), Time.fixedDeltaTime * 40f);
        rb.AddForce((GetTheBulletDir(bullet.bulletDir)) * (bullet.speed * Time.fixedDeltaTime) , ForceMode.Impulse);
    }

    private void Explosion()
    {
        GameObject Explosion = Pooling.instance.Pop("Explosion");
        Explosion.transform.position = transform.position;
        Explosion.transform.rotation = Quaternion.identity;
    }

    public void DragModification(float value)
    {
        drag = value;
    }
}
