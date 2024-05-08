using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;
using Weapon.Interface;

public class Missile : BulletBehavior,IExplosion
{
    protected LayerMask whoIsTarget;
    [SerializeField] protected float drag;
    [SerializeField] protected float timeBeforeExplosion = 2f;
    protected float time = 0;
    
    protected virtual void OnDisable()
    {
        base.OnDisable();
        time = 0;
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

    protected override void Update()
    {
        base.Update();
        time += Time.deltaTime * 1;
        if (time >= timeBeforeExplosion)
        {
            time = 0;
            Explosion();
            Pooling.instance.DePop(bullet.PoolingKeyName, gameObject);
        }
        
    }
    
    protected override void FixedUpdate()
    {
        if (!EnableMovement(bullet.isMoving)) return;
        TrackPlayer();
    }

    private void TrackPlayer()
    {
        Vector3 bulletDir = (PlayerController.Instance.transform.position - transform.position).normalized;
        
        rb.drag = drag;
        transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation (bulletDir), Time.fixedDeltaTime * 40f);
        rb.AddForce((bulletDir) * (bullet.speed * Time.fixedDeltaTime) , ForceMode.Force);
        rb.isKinematic = false;
    }
    
    public void WhoIsTheTarget(LayerMask value)
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
        this.explosion.SetDoPlayerDamage(true);
    }

    public virtual void HitScanExplosion(LayerMask newTarget)
    {
        GameObject Explosion = Pooling.instance.Pop("Explosion");
        Explosion.transform.position = transform.position;
        Explosion.transform.rotation = Quaternion.identity;
        explosion = Explosion.GetComponent<Explosion>();
        explosion.SetWhoIsTarget(newTarget);
        this.explosion.SetDoPlayerDamage(true);
        
        Pooling.instance.DelayedDePop(bullet.PoolingKeyName, gameObject, 0.05f); // DePop Missile
    }
}
