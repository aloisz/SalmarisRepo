using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using Player;
using UnityEngine;
using UnityEngine.UI;
using Weapon;

public class HUD : GenericSingletonClass<HUD>
{
    [SerializeField] private Vector2 giggleMultiplierBackground;
    [SerializeField] private Vector2 giggleMultiplier;
    [SerializeField] private Vector2 crosshairImpulseMinMax;
    [CurveRange(0,0,1,1)][SerializeField] private AnimationCurve crosshairAnimation;
    [CurveRange(0,0,1,1)][SerializeField] private AnimationCurve crosshairBombAnimation;
    [CurveRange(0,0,1,1)][SerializeField] private AnimationCurve crosshairBombScaleAnimation;
    [CurveRange(0,0,1,1)][SerializeField] private AnimationCurve crosshairBombDropDownAnimation;
    
    [SerializeField] private Image UIBackground, shieldBar, healthBar, dashBar;
    [SerializeField] private List<Image> crosshairBorders = new List<Image>();
    [SerializeField] private Image crosshairDots;
    [SerializeField] private Image crosshairBombDropdown;

    private float _giggleX;
    private float _giggleY;
    private float _rotationDots;
    
    private static readonly int ScreenGiggleX = Shader.PropertyToID("_ScreenGiggleX");
    private static readonly int ScreenGiggleY = Shader.PropertyToID("_ScreenGiggleY");

    private void Start()
    {
        CreateMaterialInstance(UIBackground);
        CreateMaterialInstance(shieldBar);
        CreateMaterialInstance(healthBar);
        CreateMaterialInstance(dashBar);
        CreateMaterialInstance(crosshairDots);
        CreateMaterialInstance(crosshairBombDropdown);
        
        foreach(var cb in crosshairBorders) CreateMaterialInstance(cb);

        PlayerHealth.Instance.onHit += DamageHealthBarEffect;
        WeaponState.Instance.barbatos.OnHudShoot += CrosshairShoot;
    }

    private void Update()
    {
        UIGiggle(UIBackground.material, giggleMultiplierBackground);
        UIGiggle(shieldBar.material, giggleMultiplier);
        UIGiggle(healthBar.material,giggleMultiplier);
        UIGiggle(dashBar.material, giggleMultiplier);
        
        UIStamina();
        UIHealthShield();
    }

    private void UIGiggle(Material mat, Vector2 multiplier)
    {
        _giggleX = Mathf.Lerp(_giggleX, PlayerInputs.Instance.rotateValue.x / 540f, Time.deltaTime * 1f);
        _giggleY = Mathf.Lerp(_giggleY, PlayerInputs.Instance.rotateValue.y / 540f, Time.deltaTime * 1f);
        
        mat.SetFloat(ScreenGiggleX, -_giggleX * multiplier.x);
        mat.SetFloat(ScreenGiggleY, -_giggleY * multiplier.y);
    }

    private void UIStamina()
    {
        dashBar.material.DOFloat(PlayerStamina.Instance.staminaValue / 100f, "_BarAmount", 0.1f);
    }
    
    private void UIHealthShield()
    {
        shieldBar.material.DOFloat(PlayerHealth.Instance.Shield / PlayerHealth.Instance.maxShield, "_BarAmount", 0.1f);
        healthBar.material.DOFloat(PlayerHealth.Instance.Health / PlayerHealth.Instance.maxHealth, "_BarAmount", 0.1f);

        shieldBar.material.DOFloat(1-(PlayerHealth.Instance.Shield / PlayerHealth.Instance.maxShield), 
            "_AlertMode", 0.1f);
        
        healthBar.material.DOFloat(1-(PlayerHealth.Instance.Health / PlayerHealth.Instance.maxHealth), 
            "_AlertMode", 0.1f);
    }

    private void DamageHealthBarEffect()
    {
        if (PlayerHealth.Instance.Shield > 0) return;
        var value = Mathf.Lerp(10f, 3f, PlayerHealth.Instance.Health / PlayerHealth.Instance.maxHealth);
        healthBar.transform.DOShakePosition(0.5f, Vector3.one * value, 120);
    }

    private void CrosshairShoot()
    {
        var wep = WeaponState.Instance.defaultWeapon;
        var wepManager = WeaponState.Instance.barbatos;
        var wepPrimary = wep.weaponMode[0];
        var wepSecondary = wep.weaponMode[1];

        if ((int)wepManager.actualWeaponModeIndex == 0)
        {
            foreach (var cb in crosshairBorders)
            {
                cb.material.SetFloat("_Offset", 0f);

                cb.material.DOFloat(Mathf.Lerp(crosshairImpulseMinMax.x, crosshairImpulseMinMax.y, wepPrimary.fireRate / 20f), 
                    "_Offset", 1/wepPrimary.fireRate).SetEase(crosshairAnimation);
            }
        }
        else if ((int)wepManager.actualWeaponModeIndex == 1)
        {
            var rect = crosshairDots.GetComponent<RectTransform>();
            //rect.localRotation *= Quaternion.Euler(new Vector3(0,0,45));
            rect.DOLocalRotateQuaternion(Quaternion.Euler(new Vector3(0, 0, rect.localRotation.eulerAngles.z - 90)), 
                1 / wepSecondary.fireRate * 0.25f).SetEase(crosshairBombAnimation);
            rect.DOScale(Vector3.one * 1.5f, 1 / wepSecondary.fireRate * 0.25f).SetEase(crosshairBombScaleAnimation);
            
            crosshairBombDropdown.material.SetFloat("_Alpha", 0f);
            crosshairBombDropdown.material.DOFloat(1f,"_Alpha", 1 / wepSecondary.fireRate * 0.1f);
            
            crosshairBombDropdown.material.SetFloat("_OffsetAmount", 0f);
            crosshairBombDropdown.material.DOFloat(0.7f,"_OffsetAmount", 1 / wepSecondary.fireRate * 0.1f).SetEase(crosshairBombDropDownAnimation)
                .OnComplete(()=>
            {
                crosshairBombDropdown.material.DOFloat(0f,"_Alpha", 1 / wepSecondary.fireRate * 0.3f);
            });
            
            foreach (var cb in crosshairBorders)
            {
                cb.material.SetFloat("_Offset", 0f);

                cb.material.DOFloat(0.025f, "_Offset", 1/wepPrimary.fireRate).SetEase(crosshairAnimation);
            }
        }
    }

    private void CreateMaterialInstance(Image imageToGetMaterialFrom)
    {
        var mat = Instantiate(imageToGetMaterialFrom.material);
        imageToGetMaterialFrom.material = mat;
    }
}
