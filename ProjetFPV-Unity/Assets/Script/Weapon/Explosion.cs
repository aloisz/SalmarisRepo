using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public float triggerForce;
    public float explosionRadius;
    public float explosionForce;
    public GameObject particle;

    private void Start()
    {
        GetComponent<SphereCollider>().radius = explosionRadius;
        Explode();
    }


    void Explode()
    {
        Collider[] surroundingObj = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider obj in surroundingObj)
        {
            Debug.Log(obj);
            var rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
        }
        Destroy(gameObject);
    }
}
