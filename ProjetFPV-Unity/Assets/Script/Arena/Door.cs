using System;
using System.Collections;
using System.Collections.Generic;
using MyAudio;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Door : MonoBehaviour
{
    public int doorID;
    public Key neededKey;

    private bool isDeactivated;

    [SerializeField] private Animator animator;

    private void Start()
    {
        if (GetComponent<SphereCollider>()) GetComponent<SphereCollider>().isTrigger = true;
        if(neededKey == null) DeactivateDoor(false);
    }

    public void DeactivateDoor(bool DoAudio)
    {
        if(animator) animator.SetTrigger("Open");
        else gameObject.SetActive(false);

        if (DoAudio)
        {
            //Audio
            AudioManager.Instance.SpawnAudio3D(gameObject.transform.position, SfxType.SFX, 30, 1,0,1, 1,0,
                AudioRolloffMode.Logarithmic, 30, 150);
        }

        isDeactivated = true;
    }

    public void ActivateDoor()
    {
        if(animator) animator.SetTrigger("Close");
        else gameObject.SetActive(true);
        
        //Audio
        AudioManager.Instance.SpawnAudio3D(gameObject.transform.position, SfxType.SFX, 29, 1,0,1, 1,0,
            AudioRolloffMode.Logarithmic, 29, 150);

        isDeactivated = false;
    }

    public void ActivateLockedDoor()
    {
        if (neededKey != null && neededKey.isPickedUp && !isDeactivated && (!neededKey.DEBUG_DONT_NEED_ARNEA_CLEARED ? neededKey.arenaTrigger.isCompleted : true))
        {
            DeactivateDoor(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            /*if (neededKey != null && neededKey.isPickedUp && !isDeactivated && (!neededKey.DEBUG_DONT_NEED_ARNEA_CLEARED ? neededKey.arenaTrigger.isCompleted : true))
            {
                DeactivateDoor();
            }*/
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
