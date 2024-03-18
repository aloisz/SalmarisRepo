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
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        Destroy(gameObject, 10);
    }

    private void FixedUpdate()
    {
        if (!EnableMovement(bullet.isMoving)) return;
        rb.velocity = (GetThePlayerDir(bullet.playerDir)) * (bullet.speed * Time.fixedDeltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (LayerMask.GetMask(LayerMask.LayerToName(collision.gameObject.layer)) == walkableMask)
        {
            bullet.isMoving = false;
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;
            Destroy(gameObject, 3);
        }
        
        if (LayerMask.GetMask(LayerMask.LayerToName(collision.gameObject.layer)) == enemyMask)
        {
            collision.transform.GetComponent<IDamage>().Hit(bullet.damage);
            Destroy(gameObject);
        }
    }

    public bool EnableMovement(bool logic)
    {
        return bullet.isMoving = logic;
    }

    public float AddVelocity(float speed)
    {
        return bullet.speed = speed;
    }

    public float AddDamage(float damage)
    {
        return bullet.damage = damage;
    }

    public Vector3 GetThePlayerDir(Vector3 dir)
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
    

