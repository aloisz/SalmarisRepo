using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;
using Weapon;
using Random = UnityEngine.Random;

public class BarbatwoBullet : BulletBehavior
{
    protected LayerMask whoIsTheTarget;
    protected float drag;
    private float rocketJumpForceApplied;

    private ProjectileType projectileType;
    private int bounceNbr;

    protected virtual void OnDisable()
    {
        base.OnDisable();
        if(!bulletShot) return;
        bulletShot = false;
        
    }
    
    // Here put following logic when bullet collide with walkableMask
    protected override void CollideWithWalkableMask(Collision collision)
    {
        switch (projectileType)
        {
            case ProjectileType.Simple:
                SimpleLogic();
                DecalSpawnerManager.Instance.SpawnDecal(transform.position, collision.contacts[0].normal, (DecalSpawnerManager.possibleDecals)Random.Range(2,4));
                break;
            case ProjectileType.Bounce:
                BounceLogic();
                break;
        }
    }

    private void SimpleLogic()
    {
        Explosion();
        trailRenderer.enabled = false;
        Pooling.instance.DePop(bullet.PoolingKeyName, gameObject);
    }

    private void BounceLogic()
    {
        Explosion();
        if(bounceNbr == 1)
        {
            trailRenderer.enabled = false;
            Pooling.instance.DePop(bullet.PoolingKeyName, gameObject);
        }
        else
        {
            bounceNbr--;
            Vector3 direction = bullet.bulletDir;
            RaycastHit hit;
            if (Physics.Raycast(transform.position, bullet.bulletDir, out hit, 1000, walkableMask))
            {
                bullet.bulletDir = Vector3.Reflect(direction, hit.normal);
                bulletShot = false;
                bullet.speed = bullet.speed / 2;
            }
        }
    }
    
    // Here put following logic when bullet collide with enemyMask
    protected override void CollideWithPlayerMask(Collision collision)
    {
        Debug.Log("qdqsd");
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
        transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation (GetBulletDir()), Time.fixedDeltaTime * 40f);
        rb.AddForce((GetBulletDir()) * (bullet.speed * Time.fixedDeltaTime) , ForceMode.Impulse);
    }

    private Explosion explosion;
    private void Explosion()
    {
        GameObject Explosion = Pooling.instance.Pop("Explosion");
        Explosion.transform.position = transform.position;
        Explosion.transform.rotation = Quaternion.identity;
        
        this.explosion = Explosion.GetComponent<Explosion>();
        this.explosion.SetDoPlayerDamage(false);
        this.explosion.SetDamage(bullet.damage);
        this.explosion.SetRocketForce(rocketJumpForceApplied);
        this.explosion.SetWhoIsTarget(whoIsTheTarget);
        explosion.SetParticleIndex(0);
    }

    public void WhoIsTheTarget(LayerMask value)
    {
        whoIsTheTarget = value;
    }

    public void DragModification(float value)
    {
        drag = value;
    }
    
    public void RocketJumpForceApplied(float value)
    {
        rocketJumpForceApplied = value;
    }

    public void DoBounce(ProjectileType type, int bounceNbr)
    {
        projectileType = type;
        this.bounceNbr = bounceNbr;
    }
}
