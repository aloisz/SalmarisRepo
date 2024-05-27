using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Weapon.Interface;

public class Missile : BulletBehavior,IExplosion
{
    protected LayerMask whoIsTarget;
    [SerializeField] protected float drag;
    
    [SerializeField] protected MeshRenderer meshRenderer;
    [SerializeField] protected Material baseMat;

    private Material _bulletMat;
    private float timer;

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnEnable()
    {
        _bulletMat = CreateMaterialInstance(baseMat);
        meshRenderer.material = _bulletMat;
        meshRenderer.material.SetFloat("_Blink", 0.1f);
        
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        meshRenderer.material.SetFloat("_Blink", 0.1f);
        base.OnDisable();
    }

    protected override void Update()
    {
        timer += Time.deltaTime;
        meshRenderer.material.SetFloat("_CustomTime", timer);
        meshRenderer.material.SetFloat("_Blink", timer / bulletLifeTime);
        base.Update();
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        DecalSpawnerManager.Instance.SpawnDecal(transform.position, collision.contacts[0].normal, DecalSpawnerManager.possibleDecals.explosionEnemy);
        base.OnCollisionEnter(collision);
    }

    // Here put following logic when bullet collide with walkableMask
    protected override void CollideWithWalkableMask(Collision collision)
    {
        Explosion();
        trailRenderer.enabled = false;
        Pooling.instance.DePop(bullet.PoolingKeyName, gameObject);
    }
    
    // Here put following logic when bullet collide with enemyMask
    protected override void CollideWithPlayerMask(Collision collision)
    {
        Explosion();
        trailRenderer.enabled = false;
        collision.transform.GetComponent<IDamage>().Hit(bullet.damage);
        Pooling.instance.DePop(bullet.PoolingKeyName, gameObject);
    }

    protected override void EventWhenBulletLifeTimeEnd()
    {
        GetComponent<CapsuleCollider>().radius = 50;
        GetComponent<CapsuleCollider>().height = 50;
        GetComponent<CapsuleCollider>().enabled = false;
        
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
        explosion.SetWhoIsTarget(playerMask);
        this.explosion.SetDamage(bullet.damage);
        this.explosion.SetDoPlayerDamage(true);
        explosion.SetParticleIndex(1);
        explosion.particles[explosion.particlesIndex].Play();
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
        explosion.particles[explosion.particlesIndex].Play();
        
        Pooling.instance.DelayedDePop(bullet.PoolingKeyName, gameObject, 0.05f); // DePop Missile
    }
    
    private Material CreateMaterialInstance(Material m)
    {
        var mat = Instantiate(m);
        return mat;
    }

    private void OnDrawGizmos()
    {
        if(Application.isPlaying) 
            Handles.Label(transform.position, meshRenderer.material.GetFloat("_Blink").ToString("F3") + $"\n{timerBulletLifeTime:F1}", new GUIStyle(){fontSize = 30});
    }
}
