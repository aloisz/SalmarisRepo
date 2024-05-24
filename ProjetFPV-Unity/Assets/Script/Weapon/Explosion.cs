using System;
using System.Collections;
using System.Collections.Generic;
using AI;
using CameraBehavior;
using Player;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class Explosion : MonoBehaviour
{
    public float damageInflicted = 20;
    [SerializeField] private AnimationCurve damageRepartition;
    [SerializeField] private AnimationCurve camShakeRepartition;
    public LayerMask explosionMask;
    public bool doDamagePlayer;
    [Space]
    
    public float explosionRadius;
    public float explosionForce;
    
    //particles 
    private int particlesIndex;
    [SerializeField]private List<ParticleSystem> particles;
    
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
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if(hasExploded) return;
        hasExploded = true;
            
        damageRepartition.AddKey(0, damageInflicted);
        damageRepartition.AddKey(explosionRadius, damageInflicted / explosionRadius);

        CameraManager.Instance.cameraJumping.CameraShake(7, .5f, camShakeRepartition,
            Vector3.Distance(PlayerController.Instance.transform.position, transform.position));
        
        Explode();
        
        particles[particlesIndex].Play();
    }


    private void Explode()
    {
        Collider[] surroundingObj = Physics.OverlapSphere(transform.position, explosionRadius, explosionMask);

        foreach (Collider obj in surroundingObj)
        {
            if (obj.GetComponent<IDamage>() != null)
            {
                var component = obj.GetComponent<IDamage>();
                if(obj.GetComponent<AI_Pawn>()) obj.GetComponent<AI_Pawn>().DisableAgent();

                if (!obj.GetComponent<PlayerController>())
                {
                    component.Hit(damageRepartition.Evaluate(Vector3.Distance(transform.position ,obj.transform.position)));
                }
            }
            
            if (obj.transform.gameObject.CompareTag("Player")) // if is player then add rocketJump value
            {
                if (PlayerController.Instance.isUnderCeiling) return;
                
                Vector3 dir = PlayerController.Instance.GetDirectionXZ(
                    PlayerController.Instance.DirectionFromCamera(
                        Helper.ConvertToV3Int(PlayerController.Instance.direction)) * 
                    (!PlayerController.Instance.isOnGround ? 2f : 0.5f));

                Vector3 shotgunImpulseVector = Vector3.up * rocketJumpForceApplied + dir;

                PlayerController.Instance._rb.AddForce(
                    shotgunImpulseVector, ForceMode.Impulse);
                
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

    public bool SetDoPlayerDamage(bool value)
    {
        return doDamagePlayer = value;
    }

    public float SetDamage(float value)
    {
        return damageInflicted = value;
    }

    public int SetParticleIndex(int value)
    {
        return particlesIndex = value;
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
