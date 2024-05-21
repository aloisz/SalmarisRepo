using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.Serialization;
using Weapon.Interface;

public class Missile : BulletBehavior,IExplosion
{
    protected LayerMask whoIsTarget;
    [SerializeField] protected float drag;
    
    protected virtual void OnDisable()
    {
        base.OnDisable();
    }
    
    // Here put following logic when bullet collide with walkableMask
    protected override void CollideWithWalkableMask(Collision collision)
    {
        /*Explosion();
        trailRenderer.enabled = false;
        Pooling.instance.DePop(bullet.PoolingKeyName, gameObject);*/
    }
    
    // Here put following logic when bullet collide with enemyMask
    protected override void CollideWithEnemyMask(Collision collision)
    {
        Explosion();
        trailRenderer.enabled = false;
        collision.transform.GetComponent<IDamage>().Hit(bullet.damage);
        Pooling.instance.DePop(bullet.PoolingKeyName, gameObject);
    }

    protected override void EventWhenBulletLifeTimeEnd()
    {
        base.EventWhenBulletLifeTimeEnd();
        Explosion();
    }
    
    protected override void FixedUpdate()
    {
        if (!EnableMovement(bullet.isMoving)) return;
        TrackPlayer();
    }

    protected virtual void TrackPlayer()
    {
        Vector3 bulletDir = (PlayerController.Instance.transform.position - transform.position).normalized;
        
        rb.drag = drag;
        transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation (bulletDir), Time.fixedDeltaTime * 40f);
        rb.AddForce((bulletDir) * (bullet.speed * Time.fixedDeltaTime) , ForceMode.Force);
        rb.isKinematic = false;
    }
    
    public virtual void WhoIsTheTarget(LayerMask value)
    {
        whoIsTarget = value;
    }
    
    protected Explosion explosion;
    public virtual void Explosion()
    {
        GameObject Explosion = Pooling.instance.Pop("Explosion");
        Explosion.transform.position = transform.position;
        Explosion.transform.rotation = Quaternion.identity;
        explosion = Explosion.GetComponent<Explosion>();
        explosion.SetWhoIsTarget(enemyMask);
        this.explosion.SetDamage(bullet.damage);
        this.explosion.SetDoPlayerDamage(true);
        explosion.SetParticleIndex(1);
    }

    public virtual void HitScanExplosion(LayerMask newTarget)
    {
        GameObject Explosion = Pooling.instance.Pop("Explosion");
        Explosion.transform.position = transform.position;
        Explosion.transform.rotation = Quaternion.identity;
        explosion = Explosion.GetComponent<Explosion>();
        explosion.SetWhoIsTarget(newTarget);
        this.explosion.SetDamage(bullet.damage);
        this.explosion.SetDoPlayerDamage(true);
        explosion.SetParticleIndex(1);
        
        Pooling.instance.DelayedDePop(bullet.PoolingKeyName, gameObject, 0.05f); // DePop Missile
    }
}
