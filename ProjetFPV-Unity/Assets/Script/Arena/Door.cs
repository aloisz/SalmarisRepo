using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Door : MonoBehaviour
{
    public int doorID;
    public Key neededKey;

    private void Start()
    {
        if(neededKey == null) DeactivateDoor();
    }

    public void DeactivateDoor()
    {
        gameObject.SetActive(false);
    }

    public void ActivateDoor()
    {
        gameObject.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (neededKey != null && neededKey.isPickedUp)
            {
                DeactivateDoor();
                Destroy(neededKey.gameObject);
            }
        }
    }

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (neededKey != null)
        {
            if (neededKey.keyID == doorID)
            {
                Handles.DrawLine(transform.position, neededKey.transform.position, 3f);
            }
        }
        
        Handles.color = Color.black;
        var style = new GUIStyle()
        {
            fontStyle = FontStyle.Bold
        };
        Handles.Label(transform.position + new Vector3(0,1,0), "Door ID : " + doorID, style);
    }
    #endif
}
