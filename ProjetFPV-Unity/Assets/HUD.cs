using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Player;
using UnityEngine;
using UnityEngine.UI;
using Weapon;

public class HUD : GenericSingletonClass<HUD>
{
    [SerializeField] private Vector2 giggleMultiplierBackground;
    [SerializeField] private Vector2 giggleMultiplier;
    [SerializeField] private AnimationCurve crosshairAnimation;
    
    [SerializeField] private Image UIBackground, shieldBar, healthBar, dashBar;
    [SerializeField] private List<Image> crosshairBorders = new List<Image>();

    private float _giggleX;
    private float _giggleY;
    private Sequence crosshairSequence;
    private static readonly int ScreenGiggleX = Shader.PropertyToID("_ScreenGiggleX");
    private static readonly int ScreenGiggleY = Shader.PropertyToID("_ScreenGiggleY");

    private void Start()
    {
        CreateMaterialInstance(UIBackground);
        CreateMaterialInstance(shieldBar);
        CreateMaterialInstance(healthBar);
        CreateMaterialInstance(dashBar);
        
        foreach(var cb in crosshairBorders) CreateMaterialInstance(cb);

        PlayerHealth.Instance.onHit += DamageHealthBarEffect;
        WeaponState.Instance.barbatos.OnShoot += CrosshairShoot;
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
        shieldBar.material.DOFloat(PlayerHealth.Instance.Shield / 100f, "_BarAmount", 0.1f);
        healthBar.material.DOFloat(PlayerHealth.Instance.Health / 100f, "_BarAmount", 0.1f);

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
        foreach (var cb in crosshairBorders)
        {
            cb.material.SetFloat("_Offset", 0f);
            crosshairSequence.Kill();
            
            crosshairSequence.Append(cb.material.DOFloat(0.1f, "_Offset", crosshairAnimation.keys[^1].time).SetEase(crosshairAnimation));
        }
    }

    private void CreateMaterialInstance(Image imageToGetMaterialFrom)
    {
        var mat = Instantiate(imageToGetMaterialFrom.material);
        imageToGetMaterialFrom.material = mat;
    }
}
