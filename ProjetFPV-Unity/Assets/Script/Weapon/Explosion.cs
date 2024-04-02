using System;
using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEditor;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public float damageInflicted = 20;
    [Space]
    public float explosionRadius;
    public float explosionForce;
    public ParticleSystem particle;

    private bool hasExploded;

    private void OnTriggerEnter(Collider other)
    {
        if(hasExploded) return;
        hasExploded = true;
        
        GetComponent<SphereCollider>().radius = explosionRadius;
        Explode();
        particle.Play();
    }

    private void OnEnable()
    {
        hasExploded = false;
    }

    private void Explode()
    {
        Collider[] surroundingObj = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider obj in surroundingObj)
        {
            var enemy = obj.GetComponent<AI_Pawn>();
            if (enemy != null)
            {
                enemy.DisableAgent();
                enemy.Hit(damageInflicted);
            }
            
            
            var rb = obj.GetComponent<Rigidbody>();
            if (rb != null) rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
        }
        Pooling.instance.DelayedDePop("Explosion", gameObject,2);
    }

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        var tr = transform;
        var pos = tr.position;
        // display an orange disc where the object is
        var color = new Color32(255, 125, 0, 20);
        Handles.color = color;
        Handles.DrawSolidDisc(pos, tr.up, explosionRadius);
    }
    #endif
    
}
