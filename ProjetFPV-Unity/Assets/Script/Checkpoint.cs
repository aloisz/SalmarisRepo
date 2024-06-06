using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private int ID;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && ID == GameManager.Instance.currentCheckpointIndex + 1)
        {
            CareTaker.Instance.SaveGameState();
            GameManager.Instance.currentCheckpointIndex = ID;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        var guiStyle = new GUIStyle()
        {
            normal = new GUIStyleState()
            {
                textColor = Color.white
            },
            fontSize = 30
        };
        Handles.Label(transform.position, "ID : "+ ID, guiStyle);
        Helper.DrawBoxCollider(Color.red, transform, GetComponent<BoxCollider>(), 0.5f);
    }
    
    #endif
}
