using System;
using System.Collections;
using System.Collections.Generic;
using AI;
using Player;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public float damageInflicted = 20;
    [SerializeField] private AnimationCurve damageRepartition;
    public LayerMask explosionMask;
    public bool doDamagePlayer;
    [Space]
    
    public float explosionRadius;
    public float explosionForce;
    public ParticleSystem particle;

    [Header("RocketJump")] /*[SerializeField]
    private LayerMask PlayerMask;*/
    private bool canRocketJump;
    private float rocketJumpForceApplied;

    private SphereCollider sphereColliderRadius;

    private float baseExplosionRadius;
    private float baseExplosionForce;
    
    private bool hasExploded = false;

    private void Awake()
    {
        sphereColliderRadius = GetComponent<SphereCollider>();
        baseExplosionForce = explosionForce;
        baseExplosionRadius = explosionRadius;
        sphereColliderRadius.radius = explosionRadius;
    }
    
    private void OnDisable()
    {
        hasExploded = false;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        
        SetRadius(baseExplosionRadius);
        SetForce(baseExplosionForce);
        
        SetRocketJump(false);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if(hasExploded) return;
        hasExploded = true;
            
        damageRepartition.AddKey(0, damageInflicted);
        damageRepartition.AddKey(explosionRadius, damageInflicted / explosionRadius);
        
        Explode();
        particle.Play();
    }


    private void Explode()
    {
        Collider[] surroundingObj = Physics.OverlapSphere(transform.position, explosionRadius, explosionMask);

        foreach (Collider obj in surroundingObj)
        {
            if (obj.GetComponent<AI_Pawn>() != null)
            {
                var enemy = obj.GetComponent<AI_Pawn>();
                enemy.DisableAgent();
                enemy.Hit(damageRepartition.Evaluate(Vector3.Distance(transform.position ,obj.transform.position)));
            }
            
            if (obj.transform.gameObject.CompareTag("Player")) // if is player then add rocketJump value
            {
                Vector3 shotgunImpulseVector = ((PlayerController.Instance.transform.position + Vector3.up) - transform.position).normalized * rocketJumpForceApplied;
                PlayerController.Instance.shotgunExternalForce = shotgunImpulseVector;
                if(doDamagePlayer)
                {
                    obj.GetComponent<PlayerHealth>().Hit(
                    damageRepartition.Evaluate(Vector3.Distance(transform.position ,obj.transform.position)));
                }
            }
            else
            {
                if (obj.GetComponent<Rigidbody>() == null) continue;
                var rb = obj.GetComponent<Rigidbody>();
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
        }
        Pooling.instance.DelayedDePop("Explosion", gameObject, 2);
    }


    public void SetWhoIsTarget(LayerMask value)
    {
        explosionMask = value;
    }
    
    public void SetRadius(float value)
    {
        explosionRadius = value;
        sphereColliderRadius.radius = value;
    }

    public void SetForce(float value)
    {
        explosionForce = value;
    }

    public float SetRocketForce(float value)
    {
        return rocketJumpForceApplied = value;
    }
    
    public bool SetRocketJump(bool value)
    {
        return canRocketJump = value;
    }

    public bool SetDoPlayerDamage(bool value)
    {
        return doDamagePlayer = value;
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
