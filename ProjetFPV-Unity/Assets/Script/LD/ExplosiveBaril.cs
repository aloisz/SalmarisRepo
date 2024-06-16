using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Weapon.Interface;

public class ExplosiveBaril : MonoBehaviour, IExplosion, IDamage
{
    [SerializeField] private float health = 100;
    [SerializeField] private LayerMask explosionMask;
    [SerializeField] private float damage = 35f;
    
    private Explosion _explosion;
    private bool _isExploded;

    public void Explosion()
    {
        GameObject Explosion = Pooling.Instance.Pop("Explosion");
        Explosion.transform.position = transform.position;
        Explosion.transform.rotation = Quaternion.identity;
        _explosion = Explosion.GetComponent<Explosion>();
        _explosion.SetWhoIsTarget(explosionMask);
        _explosion.SetDoPlayerDamage(true);
        _explosion.SetDamage(damage);
        _explosion.SetParticleIndex(1);
    }

    public void HitScanExplosion(LayerMask newTarget)
    {
        
    }

    public void Hit(float damageInflicted)
    {
        health -= damageInflicted;
        if (health <= 0 && !_isExploded)
        {
            _isExploded = true;
            Explosion();
            Destroy(gameObject);
        }
    }
}
