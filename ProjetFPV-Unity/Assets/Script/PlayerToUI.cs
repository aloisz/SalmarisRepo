using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerToUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ammo, ammoMax, stamina, weaponName;
    [SerializeField] private Image staminaCharge;

    // Update is called once per frame
    void Update()
    {
        var weapons = PlayerInputs.Instance.weapons;
        var weaponFromIndex = weapons[0];
        var weaponModeIndex = (int)weaponFromIndex.actualWeaponModeIndex;
        var weaponModeFromIndex = weaponFromIndex.so_Weapon.weaponMode[weaponModeIndex];
        
        //Weapon informations
        ammo.text = weaponModeFromIndex.isBulletInfinite ? "/" : weaponFromIndex.actualNumberOfBullet.ToString();
        ammoMax.text = "/" + (weaponModeFromIndex.isBulletInfinite ? "/" : weaponModeFromIndex.numberOfBullet.ToString());
        weaponName.text = weaponFromIndex.so_Weapon.weaponName;

        //Stamina
        stamina.text = Mathf.RoundToInt(PlayerStamina.Instance.staminaValue).ToString();
        staminaCharge.fillAmount = PlayerStamina.Instance.staminaValue / 100f;
    }
}
