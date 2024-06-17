using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.Serialization;

public class UpgradeModuleInteraction : MonoBehaviour, IInteract
{
    private UpgradeModule _upgradeModule;
    
    public bool alreadyInteracted;

    private void Awake()
    {
        _upgradeModule = GetComponent<UpgradeModule>();
    }

    
    public void Interact()
    {
        if (alreadyInteracted) return;

        alreadyInteracted = true;
        StartCoroutine(WaitForPlayerState());
    }

    private IEnumerator WaitForPlayerState()
    {
        PlayerController.Instance.isMoving = false;
        yield return new WaitForSeconds(0.01f);
        _upgradeModule.InitMenu();
    }
}
