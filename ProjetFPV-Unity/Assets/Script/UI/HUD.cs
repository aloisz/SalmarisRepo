using System;
using System.Collections;
using System.Collections.Generic;
using Coffee.UIExtensions;
using DG.Tweening;
using NaughtyAttributes;
using Player;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Weapon;

public class HUD : GenericSingletonClass<HUD>
{
    [Header("Values")]
    [SerializeField] private Vector2 giggleMultiplierBackground;
    [SerializeField] private Vector2 giggleMultiplier;
    [SerializeField] private Vector2 crosshairImpulseMinMax;
    [SerializeField] [Range(0f,3f)] private float damageMaxIntensity;
    [SerializeField] private float damageDisplayDuration;
    [SerializeField] private float dispersionDividerBasedOnWepSettings = 3f;
    [SerializeField] private float hitMarkerOffset = 3f;
    
    [Header("Curves")]
    [CurveRange(0,0,1,1)][SerializeField] private AnimationCurve crosshairAnimation;
    [CurveRange(0,0,1,1)][SerializeField] private AnimationCurve crosshairBombAnimation;
    [CurveRange(0,0,1,1)][SerializeField] private AnimationCurve crosshairBombScaleAnimation;
    [CurveRange(0,0,1,1)][SerializeField] private AnimationCurve crosshairBombDropDownAnimation;

    [Header("Components")]
    [SerializeField] private Image UIBackground;
    
    [SerializeField] private Image shieldBar;
    [SerializeField] private Image healthBar;
    [SerializeField] private Image dashBar;
    [SerializeField] private Image rageBar;
    
    [SerializeField] private Image crosshairDots;
    [SerializeField] private Image crosshairBombDropdown;
    [SerializeField] private GameObject crosshairParent;
    
    [SerializeField] private Image deform;
    [SerializeField] private Image vitals;
    
    [SerializeField] private Image speedEffect;
    [SerializeField] private Image slideEffect;
    
    [SerializeField] private Image reload;
    
    [Header("Components Lists")]
    [SerializeField] private List<Image> crosshairBorders = new List<Image>();
    [SerializeField] private ParticleSystem[] dashParticleSystems;
    [SerializeField] private ParticleSystem[] dashParticleSystemsDots;
    [SerializeField] private Image[] dashDots;
    [SerializeField] private UIParticle[] hitmarkerParticleSystems;
    [SerializeField] private UIParticle[] hitmarkerParticleSystemsLethal;

    private float _giggleX;
    private float _giggleY;
    private float _rotationDots;
    private float _timerDamageDisplay;
    private float _crossProductDamageRight;
    private float _crossProductDamageLeft;

    private Vector3 _basePositionHealthBar;
    private Vector3 _basePositionShieldBar;
    
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
        CreateMaterialInstance(deform);
        CreateMaterialInstance(vitals);
        CreateMaterialInstance(speedEffect);
        CreateMaterialInstance(slideEffect);

        HitMarkerSetupPosition(hitMarkerOffset);
        
        foreach(var cb in crosshairBorders) CreateMaterialInstance(cb);

        PlayerHealth.Instance.onHit += DamageBarEffect;
        PlayerHealth.Instance.onHit += UpdateDamageUI;
        
        WeaponState.Instance.barbatos.OnHudShoot += CrosshairShoot;
        WeaponState.Instance.barbatos.onHitEnemy += HitMarkerPlay;
        WeaponState.Instance.barbatos.onHitEnemyLethal += HitMarkerPlayLethal;

        _basePositionHealthBar = healthBar.transform.localPosition;
        _basePositionShieldBar = shieldBar.transform.localPosition;

        InitCrosshairBorders();
    }

    private void Update()
    {
        UIGiggle(UIBackground.material, giggleMultiplierBackground);
        UIGiggle(shieldBar.material, giggleMultiplier);
        UIGiggle(healthBar.material,giggleMultiplier);
        UIGiggle(dashBar.material, giggleMultiplier);
        UIGiggle(deform.material, giggleMultiplierBackground);
        UIGiggle(vitals.material, giggleMultiplierBackground);
        
        UIStamina();
        UIHealthShield();
        UpdateShatteredMask();
        SlideEffect();
        UpdateDashDots();
        UpdateReloadCircle();

        rageBar.fillAmount = PlayerKillStreak.Instance.KillStreak / PlayerKillStreak.Instance.maxKillStreak;
        rageBar.color = PlayerKillStreak.Instance.isInRageMode ? Color.red : Color.white;
        
        _timerDamageDisplay.DecreaseTimerIfPositive();
        
        deform.material.SetFloat("_DamageRight", _crossProductDamageRight * Mathf.Lerp(0,1,_timerDamageDisplay / damageDisplayDuration));
        deform.material.SetFloat("_DamageLeft", _crossProductDamageLeft * Mathf.Lerp(0,1,_timerDamageDisplay / damageDisplayDuration));
        
        deform.material.SetFloat("_SpeedAlpha", Mathf.Lerp(0f, 0.225f, (PlayerController.Instance._rb.velocity.magnitude / 30f)));
        
        vitals.material.SetFloat("_AlertMode", 
            Mathf.Lerp(1, 0, PlayerHealth.Instance.Health / PlayerHealth.Instance.maxHealth));
        
        speedEffect.material.SetFloat("_Alpha", Mathf.Lerp(0f,0.4f, 
            (PlayerController.Instance._rb.velocity.magnitude - 15f) / (PlayerController.Instance.playerScriptable.maxRigidbodyVelocity)));
    }

    private void UIGiggle(Material mat, Vector2 multiplier)
    {
        _giggleX = Mathf.Lerp(_giggleX, PlayerInputs.Instance.rotateValue.x / 540f, Time.deltaTime * 1f);
        _giggleY = Mathf.Lerp(_giggleY, (PlayerInputs.Instance.rotateValue.y / 540f) + 
                                        (-PlayerController.Instance._rb.velocity.y * multiplier.y) / 700f, Time.deltaTime * 1f);
        
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

    public void PlayDashVFX()
    {
        int index = 0;
        float stamina = PlayerStamina.Instance.staminaValue;
        
        if (Mathf.RoundToInt(stamina) < 33)
        {
            index = 0;
        }

        if (Mathf.RoundToInt(stamina) is > 33 and < 66)
        {
            index = 1;
        }

        if (Mathf.RoundToInt(stamina) is > 66 and < 100)
        {
            index = 2;
        }

        dashParticleSystems[index].Stop();
        dashParticleSystems[index].Play();
        
        dashParticleSystemsDots[index].Stop();
        dashParticleSystemsDots[index].Play();
    }

    private void DamageBarEffect()
    {
        switch (PlayerHealth.Instance.Shield)
        {
            case <= 0:
            {
                var value = Mathf.Lerp(10f, 3f, PlayerHealth.Instance.Health / PlayerHealth.Instance.maxHealth);
                healthBar.transform.DOShakePosition(0.5f, Vector3.one * value, 120).OnComplete(() =>
                {
                    healthBar.transform.DOLocalMove(_basePositionHealthBar, 0.1f);
                });
                break;
            }
            case > 0:
            {
                var value = Mathf.Lerp(3f, 0.5f, PlayerHealth.Instance.Shield / PlayerHealth.Instance.maxShield);
                shieldBar.transform.DOShakePosition(0.5f, Vector3.one * value, 120).OnComplete(() =>
                {
                    shieldBar.transform.DOLocalMove(_basePositionShieldBar, 0.1f);
                });
                break;
            }
        }
    }

    private void CrosshairShoot()
    {
        var wep = WeaponState.Instance.defaultWeapon;
        var wepManager = WeaponState.Instance.barbatos;
        var wepPrimary = wep.weaponMode[0];
        var wepSecondary = wep.weaponMode[1];

        var fireRateMultiplier = PlayerKillStreak.Instance.fireRateBoost;

        if ((int)wepManager.actualWeaponModeIndex == 0)
        {
            foreach (var cb in crosshairBorders)
            {
                var averageY = wepPrimary.yAxisDispersion.magnitude;
                var averageZ = wepPrimary.zAxisDispersion.magnitude;
                var dispersion = (averageY + averageZ) / dispersionDividerBasedOnWepSettings;

                var maxDispersion = 30f;
                
                cb.material.SetFloat("_Offset", dispersion / maxDispersion);
                cb.material.DOFloat(Mathf.Lerp(0,1, dispersion / maxDispersion) * 1.75f, 
                    "_Offset", 1 / (wepPrimary.fireRate * fireRateMultiplier)).SetEase(crosshairAnimation);
            }
            
            var rect = crosshairDots.GetComponent<RectTransform>();
            //rect.localRotation *= Quaternion.Euler(new Vector3(0,0,45));
            rect.rotation = new Quaternion(0,0,0,0);
            rect.DOLocalRotateQuaternion(Quaternion.Euler(new Vector3(0, 0, rect.localRotation.eulerAngles.z + 90)), 
                1 / (wepPrimary.fireRate * fireRateMultiplier)).SetEase(crosshairBombAnimation);
            rect.DOScale(Vector3.one * 1.5f, 1 / (wepPrimary.fireRate * fireRateMultiplier)).SetEase(crosshairBombScaleAnimation);
        }
        else if ((int)wepManager.actualWeaponModeIndex == 1)
        {
            var rect = crosshairDots.GetComponent<RectTransform>();
            //rect.localRotation *= Quaternion.Euler(new Vector3(0,0,45));
            rect.rotation = new Quaternion(0,0,0,0);
            rect.DOLocalRotateQuaternion(Quaternion.Euler(new Vector3(0, 0, rect.localRotation.eulerAngles.z - 90)), 
                1 / (wepSecondary.fireRate * fireRateMultiplier) * 0.25f).SetEase(crosshairBombAnimation);
            rect.DOScale(Vector3.one * 1.5f, 1 / (wepSecondary.fireRate * fireRateMultiplier) * 0.25f).SetEase(crosshairBombScaleAnimation);
            
            crosshairBombDropdown.material.SetFloat("_Alpha", 0f);
            crosshairBombDropdown.material.DOFloat(1f,"_Alpha", 1 / (wepSecondary.fireRate * fireRateMultiplier) * 0.1f);
            
            crosshairBombDropdown.material.SetFloat("_OffsetAmount", 0f);
            crosshairBombDropdown.material.DOFloat(0.7f,"_OffsetAmount", 1 / (wepSecondary.fireRate * fireRateMultiplier) * 0.1f).SetEase(crosshairBombDropDownAnimation)
                .OnComplete(()=>
            {
                crosshairBombDropdown.material.DOFloat(0f,"_Alpha", 1 / (wepSecondary.fireRate * fireRateMultiplier) * 0.3f);
            });
            
            foreach (var cb in crosshairBorders)
            {
                var averageY = wepPrimary.yAxisDispersion.magnitude;
                var averageZ = wepPrimary.zAxisDispersion.magnitude;
                var dispersion = (averageY + averageZ) / dispersionDividerBasedOnWepSettings;

                var maxDispersion = 30f;
                
                cb.material.SetFloat("_Offset", dispersion / maxDispersion);
                cb.material.DOFloat(Mathf.Lerp(0,1, dispersion / maxDispersion) * 1.75f, 
                    "_Offset", 1 / (wepPrimary.fireRate * fireRateMultiplier)).SetEase(crosshairAnimation);
            }
        }
    }

    private void InitCrosshairBorders()
    {
        var wep = WeaponState.Instance.defaultWeapon;
        var wepPrimary = wep.weaponMode[0];
        
        foreach (var cb in crosshairBorders)
        {
            var averageY = wepPrimary.yAxisDispersion.magnitude;
            var averageZ = wepPrimary.zAxisDispersion.magnitude;
            var dispersion = (averageY + averageZ) / 2f;

            var maxDispersion = 30f;
                
            cb.material.SetFloat("_Offset", dispersion / maxDispersion);
        }
    }

    private void CreateMaterialInstance(Image imageToGetMaterialFrom)
    {
        var mat = Instantiate(imageToGetMaterialFrom.material);
        imageToGetMaterialFrom.material = mat;
    }

    private void UpdateDamageUI()
    {
        deform.material.SetFloat("_DamageLeft", 0f);
        deform.material.SetFloat("_DamageRight", 0f);

        _timerDamageDisplay = damageDisplayDuration;
        
        var cross = CheckDamageDirection(Camera.main.transform, PlayerHealth.Instance.lastEnemyPosition);
        
        float vNormalized = (cross + 1f) / 2f; // Normalize t to range from 0 to 1
        _crossProductDamageRight = Mathf.Lerp(damageMaxIntensity, 0, vNormalized);
        _crossProductDamageLeft = Mathf.Lerp(0, damageMaxIntensity, vNormalized);
    }

    private void UpdateShatteredMask()
    {
        deform.material.SetFloat("_ShatteredMaskAlpha", Mathf.Lerp(0, 1, (PlayerHealth.Instance.Health + PlayerHealth.Instance.Shield) 
                                                                         / (PlayerHealth.Instance.maxHealth + PlayerHealth.Instance.maxShield)));
    }
    
    // This function checks the relative position of the 'damageSource' relative to the camera's forward direction
    public float CheckDamageDirection(Transform cameraTransform, Vector3 damageSource)
    {
        // Get the direction the camera is facing
        Vector3 cameraForward = cameraTransform.forward;
        
        // Calculate the vector from the camera to the damage source
        Vector3 toDamageSource = damageSource - cameraTransform.position;
        
        // Ignore vertical differences by setting y to 0
        cameraForward.y = 0;
        toDamageSource.y = 0;

        // Normalize the direction vectors
        cameraForward.Normalize();
        toDamageSource.Normalize();

        // Get the cross product of the camera's forward direction and the vector to the damage source
        Vector3 crossProduct = Vector3.Cross(cameraForward, toDamageSource);

        // Calculate the angle between the forward direction and the direction to the damage source
        float angle = Vector3.Angle(cameraForward, toDamageSource);

        // Determine the sign based on the cross product
        float sign = Mathf.Sign(crossProduct.y);

        // Combine the sign and angle to get a signed angle between -180 and 180 degrees
        float signedAngle = sign * angle;

        // Normalize the signed angle to be between -1 and 1
        float result = signedAngle / 180.0f;

        return result;
    }

    private void HitMarkerPlay()
    {
        foreach (var ps in hitmarkerParticleSystems)
        {
            ps.Stop();
            ps.Play();
        }
    }
    
    private void HitMarkerPlayLethal()
    {
        foreach (var ps in hitmarkerParticleSystemsLethal)
        {
            if(!ps.particles[0].isPlaying) ps.Play();
        }
    }
    
    private void HitMarkerSetupPosition(float offset)
    {
        hitmarkerParticleSystems[0].rectTransform.anchoredPosition = new Vector2(offset, offset);
        hitmarkerParticleSystems[1].rectTransform.anchoredPosition = new Vector2(-offset, -offset);
        hitmarkerParticleSystems[2].rectTransform.anchoredPosition = new Vector2(-offset, offset);
        hitmarkerParticleSystems[3].rectTransform.anchoredPosition = new Vector2(offset, -offset);

        hitmarkerParticleSystemsLethal[0].rectTransform.anchoredPosition = new Vector2(offset, offset);
        hitmarkerParticleSystemsLethal[1].rectTransform.anchoredPosition = new Vector2(-offset, -offset);
        hitmarkerParticleSystemsLethal[2].rectTransform.anchoredPosition = new Vector2(-offset, offset);
        hitmarkerParticleSystemsLethal[3].rectTransform.anchoredPosition = new Vector2(offset, -offset);
    }

    private void SlideEffect()
    {
        if (!PlayerController.Instance.isSliding)
        {
            slideEffect.material.SetFloat("_Alpha", 0f);
            return;
        }
        slideEffect.material.SetFloat("_Alpha", Mathf.Lerp(0,1,PlayerController.Instance._rb.velocity.magnitude
                                                               / (PlayerController.Instance.playerScriptable.maxRigidbodyVelocity / 2f)));
        slideEffect.material.SetFloat("_Speed", Mathf.Lerp(0,1,PlayerController.Instance._rb.velocity.magnitude
                                                               / (PlayerController.Instance.playerScriptable.maxRigidbodyVelocity / 2f)));
    }

    private void UpdateDashDots()
    {
        float stamina = PlayerStamina.Instance.staminaValue;
        Color disabledColor = new Color(1, 1, 1, 0.02f);

        dashDots[0].color = (stamina < 33) ? disabledColor : Color.white;
        dashDots[1].color = (stamina < 66) ? disabledColor : Color.white;
        dashDots[2].color = (stamina < 98) ? disabledColor : Color.white;
    }

    private void UpdateReloadCircle()
    {
        var wep = WeaponState.Instance.defaultWeapon;
        var wepManager = WeaponState.Instance.barbatos;
        var wepPrimary = wep.weaponMode[0];

        if (wepManager.isReloading)
        {
            reload.fillAmount = Mathf.Lerp(0, 1, wepManager.timeElapsedReload / (wepPrimary.timeToReload / PlayerKillStreak.Instance.reloadBoost));
        }
        
        crosshairParent.SetActive(reload.fillAmount > 0.99f);
        reload.enabled = reload.fillAmount < 0.99f;
    }
}
