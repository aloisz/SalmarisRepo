using System;
using System.Collections;
using System.Collections.Generic;
using CameraBehavior;
using UnityEngine;
using UnityEngine.Serialization;
using Weapon.Interface;

public class BulletBehavior : MonoBehaviour, IBulletBehavior
{
    public Bullet bullet;
    public LayerMask walkableMask;
    public LayerMask playerMask;
    public LayerMask bulletMask;
    
    [SerializeField] protected float bulletLifeTime;
    protected float timerBulletLifeTime = 0;

    [Header("Camera Shake when hit")] 
    [SerializeField] private float shakeDuration = .1f;
    [SerializeField] private float shakeMagnitude = 20f;
    [SerializeField] private float shakeFrequency = .5f;
    [SerializeField] private float power = 4;
    
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
        
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        timerBulletLifeTime = 0;
    }

    protected virtual void OnDisable()
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        timerBulletLifeTime = 0;
        
        UseGravity(false);
        GravityApplied(0);
        EnableMovement(false);
        AddVelocity(0);
        AddDamage(0);
        SetTheBulletDir(Vector3.zero);

        rb.isKinematic = false;
        rb.velocity = Vector3.zero;
        rb.rotation = Quaternion.identity;
        
        trailRenderer.Clear();
    }

    protected virtual void Start(){}

    protected virtual void FixedUpdate()
    {
        if (!EnableMovement(bullet.isMoving)) return;
        rb.velocity = (SetTheBulletDir(bullet.bulletDir)) * (bullet.speed * Time.fixedDeltaTime);
        rb.isKinematic = false;
    }

    protected virtual void Update()
    {
        timerBulletLifeTime += Time.deltaTime;
        if (timerBulletLifeTime >= bulletLifeTime)
        {
            timerBulletLifeTime = 0;
            EventWhenBulletLifeTimeEnd();
            Pooling.instance.DePop(bullet.PoolingKeyName, gameObject);
        }
    }
    
    protected virtual void EventWhenBulletLifeTimeEnd() {}

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (LayerMask.GetMask(LayerMask.LayerToName(collision.gameObject.layer)) == walkableMask)
        {
            CollideWithWalkableMask(collision);
        }
        
        if (LayerMask.GetMask(LayerMask.LayerToName(collision.gameObject.layer)) == playerMask)
        {
            CollideWithPlayerMask(collision);
        }

        if (LayerMask.GetMask(LayerMask.LayerToName(collision.gameObject.layer)) == bulletMask) // bullet layers
        {
            CollideWithWalkableMask(collision);
            AddVelocity(0);
            rb.isKinematic = true;
        }
    }

    // Here put following logic when bullet collide with walkableMask
    protected virtual void CollideWithWalkableMask(Collision collision)
    {
        Pooling.instance.DePop(bullet.PoolingKeyName, gameObject);
    }
    
    // Here put following logic when bullet collide with enemyMask
    protected virtual void CollideWithPlayerMask(Collision collision)
    {
        collision.transform.GetComponent<IDamage>().Hit(bullet.damage);
        Pooling.instance.DePop(bullet.PoolingKeyName, gameObject);
        CameraShake.Instance.ShakeCamera(false, shakeDuration, shakeMagnitude, shakeFrequency, true, power);
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

    public virtual Vector3 GetBulletDir()
    {
        return bullet.bulletDir;
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
    

