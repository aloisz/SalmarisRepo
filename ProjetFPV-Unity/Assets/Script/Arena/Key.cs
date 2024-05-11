using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Key : MonoBehaviour
{
    public int keyID;
    [HideInInspector] public ArenaTrigger arenaTrigger;
    [HideInInspector] public bool isPickedUp;

    private SphereCollider _collider;

    private void Start()
    {
        _collider = GetComponent<SphereCollider>();
    }

    public void DeactivateKey()
    {
        gameObject.SetActive(false);
    }
    
    public void ActivateKey()
    {
        gameObject.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPickedUp = true;
            DeactivateKey();
        }
    }
    
    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.color = Color.black;
        var style = new GUIStyle()
        {
            fontStyle = FontStyle.Bold
        };
        Handles.Label(transform.position + new Vector3(0,1,0), "Key ID : " + keyID, style);
    }
    #endif
}
