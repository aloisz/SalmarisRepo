using System;
using System.Collections;
using System.Collections.Generic;
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
        
        _upgradeModule.InitMenu();
        alreadyInteracted = true;
    }
}
