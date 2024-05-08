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
    
    [SerializeField] protected float bulletLifeTime;
    // Components
    protected Rigidbody rb;
    protected TrailRenderer trailRenderer;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        trailRenderer = GetComponent<TrailRenderer>();
    }

    protected virtual void OnEnable()
    {
        trailRenderer.enabled = true;
    }

    protected virtual void OnDisable()
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        
        UseGravity(false);
        GravityApplied(0);
        EnableMovement(false);
        AddVelocity(0);
        AddDamage(0);
        SetTheBulletDir(transform.forward);
    }

    protected virtual void Start(){}

    protected virtual void FixedUpdate()
    {
        if (!EnableMovement(bullet.isMoving)) return;
        rb.velocity = (SetTheBulletDir(bullet.bulletDir)) * (bullet.speed * Time.fixedDeltaTime);
        rb.isKinematic = false;
    }

    protected virtual void Update() { }

    protected virtual void OnCollisionEnter(Collision collision)
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
    protected virtual void CollideWithWalkableMask(Collision collision)
    {
        Pooling.instance.DePop(bullet.PoolingKeyName, gameObject);
    }
    
    // Here put following logic when bullet collide with enemyMask
    protected virtual void CollideWithEnemyMask(Collision collision)
    {
        collision.transform.GetComponent<IDamage>().Hit(bullet.damage);
        Pooling.instance.DePop(bullet.PoolingKeyName, gameObject);
    }
    
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

    public string PoolingKeyName(string key)
    {
        return bullet.PoolingKeyName = key;
    }

    public virtual Vector3 SetTheBulletDir(Vector3 dir)
    {
        return bullet.bulletDir = dir;
    }

    public bool UseGravity(bool condition)
    {
        if (condition) return rb.useGravity = true;
        else return rb.useGravity = false;
    }

    public float GravityApplied(float value)
    {
        if (!rb.useGravity)
        {
            return 0;
        }
        else
        {
            return bullet.gravityApplied = value;
        }
    }
}

[System.Serializable]
public class Bullet
{
    [HideInInspector]public string PoolingKeyName;
    public Vector3 bulletDir;
    public bool isMoving = false;
    public float speed;
    public float damage;
    public float gravityApplied;
}
    

