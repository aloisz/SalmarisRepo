using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Weapon;

public class UpgradeButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI upgradeName;
    [SerializeField] private TextMeshProUGUI upgradeCost;
    [SerializeField] private TextMeshProUGUI upgradeDescription;
    
    [SerializeField] private Image upgradeIcon;

    private SO_WeaponMode weaponMode;

    public void InitUpgradeButton(SO_WeaponMode mode)
    {
        weaponMode = mode;
        
        upgradeName.text = weaponMode.modeName;
        upgradeCost.text = $"{weaponMode.modeCostToBuy}$";
        upgradeDescription.text = weaponMode.modeDescription;

        upgradeIcon.sprite = weaponMode.modeIcon;
    }

    public void UpgradeWeapon(int modeIndex, WeaponMode mode)
    {
        
    }
}
