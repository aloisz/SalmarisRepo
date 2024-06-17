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

    private bool _alreadyEncounterLockedDoor;

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
                AudioRolloffMode.Linear, 30, 100);
        }

        isDeactivated = true;
    }

    public void ActivateDoor()
    {
        if(animator) animator.SetTrigger("Close");
        else gameObject.SetActive(true);
        
        //Audio
        AudioManager.Instance.SpawnAudio3D(gameObject.transform.position, SfxType.SFX, 29, 1,0,1, 1,0,
            AudioRolloffMode.Linear, 29, 100);

        isDeactivated = false;
    }

    public void ActivateLockedDoor()
    {
        if (neededKey != null && neededKey.isPickedUp && !isDeactivated && neededKey.arenaTrigger.isCompleted)
        {
            VoicelineManager.Instance.CallOpenDoorKeyVoiceLine();
            DeactivateDoor(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && neededKey != null)
        {
            if (!_alreadyEncounterLockedDoor)
            {
                VoicelineManager.Instance.CallLockedDoorVoiceLine();
                _alreadyEncounterLockedDoor = true;
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
