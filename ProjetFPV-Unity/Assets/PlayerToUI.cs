using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerToUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ammo, ammoMax, stamina;
    [SerializeField] private Image staminaCharge;
    
    // Update is called once per frame
    void Update()
    {
        var weaponIndex = PlayerInputs.Instance.GetIndexByBoolean(PlayerInputs.Instance.isOnMainWeapon);
        var weapons = PlayerInputs.Instance.weapons;
        var weaponMode = PlayerInputs.Instance.weapons[weaponIndex];
        var weaponModeIndex = (int)weaponMode.actualWeaponModeIndex;
        
        ammo.text = weapons[weaponIndex].actualNumberOfBullet.ToString();
        ammoMax.text = weaponMode.so_Weapon.weaponMode[weaponModeIndex].numberOfBullet.ToString();

        stamina.text = Mathf.RoundToInt(PlayerStamina.Instance.staminaValue).ToString();
        
        staminaCharge.fillAmount = PlayerStamina.Instance.staminaValue / 100f;
    }
}
