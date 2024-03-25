using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Weapon.Interface;

public class BulletBehavior : MonoBehaviour, IBulletBehavior
{
    public Bullet bullet;
    public LayerMask walkableMask;
    public LayerMask enemyMask;
    // Components
    protected Rigidbody rb;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    protected virtual void FixedUpdate()
    {
        if (!EnableMovement(bullet.isMoving)) return;
        rb.velocity = (GetThePlayerDir(bullet.playerDir)) * (bullet.speed * Time.fixedDeltaTime);
        rb.isKinematic = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (LayerMask.GetMask(LayerMask.LayerToName(collision.gameObject.layer)) == walkableMask)
        {
            CollideWithWalkableMask(collision);
        }
        
        if (LayerMask.GetMask(LayerMask.LayerToName(collision.gameObject.layer)) == enemyMask)
        {
            CollideWithEnemyMask(collision);
        }
    }

    // Here put following logic when bullet collide with walkableMask
    protected virtual void CollideWithWalkableMask(Collision collision){}
    
    // Here put following logic when bullet collide with enemyMask
    protected virtual void CollideWithEnemyMask(Collision collision){}
    
    public virtual bool EnableMovement(bool logic)
    {
        return bullet.isMoving = logic;
    }

    public virtual float AddVelocity(float speed)
    {
        return bullet.speed = speed;
    }

    public virtual float AddDamage(float damage)
    {
        return bullet.damage = damage;
    }

    public virtual Vector3 GetThePlayerDir(Vector3 dir)
    {
        return bullet.playerDir = dir;
    }
}

[System.Serializable]
public class Bullet
{
    public Vector3 playerDir;
    public bool isMoving = false;
    public float speed;
    public float damage;
}
    

