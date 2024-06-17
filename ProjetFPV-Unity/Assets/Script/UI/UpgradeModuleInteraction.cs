using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.Serialization;

public class UpgradeModuleInteraction : MonoBehaviour, IInteract
{
    private bool _isInInteraction;
    private UpgradeModule _upgradeModule;
    
    public bool alreadyInteracted;

    private void Awake()
    {
        _upgradeModule = GetComponent<UpgradeModule>();
    }

    
    public void Interact()
    {
        if (_isInInteraction || alreadyInteracted) return;

        alreadyInteracted = true;
        StartCoroutine(WaitForPlayerState());
    }

    private IEnumerator WaitForPlayerState()
    {
        PlayerController.Instance.isMoving = false;
        yield return new WaitForSeconds(.5f);
        _upgradeModule.InitMenu();
    }
}
