using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;

public class DebugTP : MonoBehaviour
{
    public Vector3 tpPosition;
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other);
        if (other.transform.parent.parent.CompareTag("Player"))
        {
            PlayerController.Instance.gameObject.transform.position = tpPosition;
        }
    }
}
