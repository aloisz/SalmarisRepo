using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerToUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ammo, ammoMax, stamina, weaponName;
    [SerializeField] private Image staminaCharge, surchargeWeapon;

    private Vlad vladReference;

    private void Start()
    {
        var weapons = PlayerInputs.Instance.weapons;
        var weaponFromIndex = weapons[0];
        
        vladReference = weaponFromIndex.GetComponent<Vlad>();
    }

    // Update is called once per frame
    void Update()
    {
        var weaponIndex = PlayerInputs.Instance.GetIndexByBoolean(!PlayerInputs.Instance.isOnMainWeapon);
        var weapons = PlayerInputs.Instance.weapons;
        var weaponFromIndex = weapons[weaponIndex];
        var weaponModeIndex = (int)weaponFromIndex.actualWeaponModeIndex;
        var weaponModeFromIndex = weaponFromIndex.so_Weapon.weaponMode[weaponModeIndex];
        
        //Weapon informations
        ammo.text = weaponModeFromIndex.isBulletInfinite ? "/" : weapons[weaponIndex].actualNumberOfBullet.ToString();
        ammoMax.text = "/" + (weaponModeFromIndex.isBulletInfinite ? "/" : weaponModeFromIndex.numberOfBullet.ToString());
        weaponName.text = weaponFromIndex.so_Weapon.weaponName;

        //Stamina
        stamina.text = Mathf.RoundToInt(PlayerStamina.Instance.staminaValue).ToString();
        staminaCharge.fillAmount = PlayerStamina.Instance.staminaValue / 100f;

        //Surcharge
        surchargeWeapon.gameObject.SetActive(weaponIndex == 0);
        
        var actualOverheatValue = vladReference.vladOverheatActualValue;
        var overheatMin = vladReference.vladOverheatMin;
        var overheatMax = vladReference.vladOverheatMax;
        var overheatProportion = actualOverheatValue / overheatMax;
        var overheatMinReached = actualOverheatValue > overheatMin;

        surchargeWeapon.fillAmount = overheatProportion;
        surchargeWeapon.color = overheatMinReached ? Color.Lerp(Color.yellow , Color.red, overheatProportion)
            : Color.white;
    }
}
