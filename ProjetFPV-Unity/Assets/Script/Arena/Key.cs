using System;
using System.Collections;
using System.Collections.Generic;
using MyAudio;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Key : MonoBehaviour
{
    public bool DEBUG_DONT_NEED_ARNEA_CLEARED;
    public int keyID;
    public ArenaTrigger arenaTrigger;
    public bool isPickedUp;

    [SerializeField] private MeshRenderer mesh;
    [SerializeField] private SphereCollider _collider;
    
    private void Start()
    {
        _collider.isTrigger = true;
    }

    public void DeactivateKey()
    {
        mesh.enabled = false;
        _collider.enabled = false;
    }
    
    public void ActivateKey()
    {
        mesh.enabled = true;
        _collider.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPickedUp = true;
            DeactivateKey();
            AudioManager.Instance.SpawnAudio3D(transform.position, SfxType.SFX, 32, 1,0,1, 1,0,
                AudioRolloffMode.Logarithmic,
            10,150);
        }
    }

    public void SetKeyData(bool isPickedUp)
    {
        this.isPickedUp = isPickedUp;
        if(isPickedUp) DeactivateKey();
        else ActivateKey();
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
