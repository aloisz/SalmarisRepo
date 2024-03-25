using System;
using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEditor;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public float explosionRadius;
    public float explosionForce;
    public GameObject particle;
    private bool isOn;

    private void Start()
    {
        GetComponent<SphereCollider>().radius = explosionRadius;
        isOn = true;
    }

    private void OnDisable()
    {
        isOn = true;
    }

    private void FixedUpdate()
    {
        if (isOn)
        {
            isOn = false;
            Explode();
        }
    }

    private void Explode()
    {
        Collider[] surroundingObj = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider obj in surroundingObj)
        {
            Debug.Log(obj, this);
            
            var enemy = obj.GetComponent<AI_Pawn>();
            if (enemy != null)
            {
                enemy.DisableAgent();
            }
            
            
            var rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 dir = obj.transform.position - transform.position ;
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
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
