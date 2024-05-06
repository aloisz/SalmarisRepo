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
    [SerializeField] private TextMeshProUGUI upgradeModeIndex;
    [SerializeField] private TextMeshProUGUI upgradeCost;
    [SerializeField] private TextMeshProUGUI upgradeDescription;
    
    [SerializeField] private Image upgradeIcon;

    private SO_WeaponMode weaponMode;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(()=>UpgradeWeapon((int)weaponMode.modeIndex, weaponMode));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mode"></param>
    public void InitUpgradeButton(SO_WeaponMode mode)
    {
        weaponMode = mode;
        
        upgradeName.text = weaponMode.modeName;
        upgradeModeIndex.text = Enum.GetName(typeof(SO_WeaponMode.ShootingModeIndex), weaponMode.modeIndex);
        
        upgradeCost.text = $"{weaponMode.modeCostToBuy}$";
        upgradeDescription.text = weaponMode.modeDescription;

        upgradeIcon.sprite = weaponMode.modeIcon;
    }

    private void UpgradeWeapon(int modeIndex, SO_WeaponMode mode)
    {
        if (PlayerMoney.Instance.Money < mode.modeCostToBuy) return;
        
        PlayerMoney.Instance.DecrementMoney(mode.modeCostToBuy);
        WeaponState.Instance.barbatos.so_Weapon.weaponMode[modeIndex] = mode;
        
        UpgradeModule.Instance.QuitMenu();
    }
}
