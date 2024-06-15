using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class TriggerVoiceline : MonoBehaviour
{
    [SerializeField] private int ID;

    private bool _alreadyEntered;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !_alreadyEntered)
        {
            _alreadyEntered = true;
            VoicelineManager.Instance.CallVoiceLine(ID);
        }
    }

    private void OnDrawGizmos()
    {
        GUIStyle style = new GUIStyle()
        {
            fontSize = 35,
            fontStyle = FontStyle.Bold
        };
        style.normal.textColor = Color.white;
        
        Handles.Label(transform.position, ID.ToString(), style);
        Helper.DrawBoxCollider(Color.green, transform, GetComponent<BoxCollider>(), Vector3.zero, 0.2f);
    }
}
