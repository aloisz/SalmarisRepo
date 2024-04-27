using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MobArenaDisplay : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        var style = new GUIStyle()
        {
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            fontSize = 12
        };
        Handles.Label(transform.position + new Vector3(0,1,0), 
            $"{gameObject.name}\n{transform.parent.name}", style);
    }
}
