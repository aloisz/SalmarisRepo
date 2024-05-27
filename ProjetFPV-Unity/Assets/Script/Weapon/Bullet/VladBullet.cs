using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Weapon.Interface;

public class VladBullet : BulletBehavior, IExplosion
{
    public bool isBulletOnFire;
    private Color baseColor = Color.green;
    private Color onFireColor = Color.red;
    
    // component
    private Renderer renderer;

    protected override void Awake()
    {
        base.Awake();
        renderer = GetComponent<Renderer>();
    }

    public void IsBulletOnFire(bool isOnFire)
    {
        renderer.material.color = isOnFire ? onFireColor : baseColor;
    }


    #region Collision Logic

    protected override void CollideWithWalkableMask(Collision collision)
    {
        bullet.isMoving = false;
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
        Pooling.instance.DelayedDePop(bullet.PoolingKeyName, gameObject,7);
    }
    
    protected override void CollideWithPlayerMask(Collision collision)
    {
        collision.transform.GetComponent<IDamage>().Hit(bullet.damage);
        Pooling.instance.DePop(bullet.PoolingKeyName, gameObject);
    }

    #endregion
    

    public void Explosion()
    {
        GameObject Explosion = Pooling.instance.Pop("Explosion");
        Explosion.transform.position = transform.position;
        Explosion.transform.rotation = Quaternion.identity;
        
        Pooling.instance.DePop(bullet.PoolingKeyName, gameObject);
    }

    public void HitScanExplosion(LayerMask newTarget)
    {
        throw new System.NotImplementedException();
    }
}
