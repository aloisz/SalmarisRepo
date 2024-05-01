using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class UpgradeModuleInteraction : MonoBehaviour, IInteract
{
    private bool _isInInteraction;
    private UpgradeModule _upgradeModule;

    private void Awake()
    {
        _upgradeModule = GetComponent<UpgradeModule>();
    }

    public void Interact()
    {
        if (_isInInteraction) return;
        
        _upgradeModule.InitMenu();
    }
}
