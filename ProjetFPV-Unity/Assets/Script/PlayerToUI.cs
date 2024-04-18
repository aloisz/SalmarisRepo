using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerToUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ammo, ammoMax, stamina, weaponName;
    [SerializeField] private Image staminaCharge, healthBar, shieldBar;

    private void Start()
    {
        PlayerHealth.Instance.onHit += UpdateHealthShieldBars;
    }

    // Update is called once per frame
    void Update()
    {
        var weapons = PlayerInputs.Instance.weapons;
        var weaponFromIndex = weapons[0];
        var weaponModeIndex = (int)weaponFromIndex.actualWeaponModeIndex;
        var weaponModeFromIndex = weaponFromIndex.so_Weapon.weaponMode[weaponModeIndex];
        
        //Weapon informations
        if(ammo) ammo.text = weaponModeFromIndex.isBulletInfinite ? "/" : weaponFromIndex.actualNumberOfBullet.ToString();
        if(ammoMax) ammoMax.text = "/" + (weaponModeFromIndex.isBulletInfinite ? "/" : weaponModeFromIndex.numberOfBullet.ToString());
        if(weaponName) weaponName.text = weaponFromIndex.so_Weapon.weaponName;

        //Stamina
        if(stamina) stamina.text = Mathf.RoundToInt(PlayerStamina.Instance.staminaValue).ToString();
        if(staminaCharge) staminaCharge.fillAmount = PlayerStamina.Instance.staminaValue / 100f;
    }

    private void UpdateHealthShieldBars()
    {
        if(healthBar) healthBar.DOFillAmount(PlayerHealth.Instance.Health / PlayerHealth.Instance.maxHealth, 0.25f);
        if(shieldBar) shieldBar.DOFillAmount(PlayerHealth.Instance.Shield / PlayerHealth.Instance.maxShield, 0.25f);
    }
}
