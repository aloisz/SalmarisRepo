using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Weapon;

public class GunScreen : MonoBehaviour
{
    [SerializeField] private MeshRenderer shotgunBar;
    [SerializeField] private MeshRenderer grenadeBar;

    private Barbatos _barbatos;

    private void Start()
    {
        _barbatos = FindObjectOfType<Barbatos>();
        
        CreateMaterialInstance(ref shotgunBar);
        CreateMaterialInstance(ref grenadeBar);

        _barbatos.OnLooseAmmo += UpdateTimerValue;
    }
    
    private float _shotgunTimer;
    private float _grenadeTimer;

    void UpdateTimerValue()
    {
        if (_barbatos.actualWeaponModeIndex == WeaponMode.Primary)
            _shotgunTimer = Normalize(1/_barbatos.so_Weapon.weaponMode[0].fireRate, 0, 1/_barbatos.so_Weapon.weaponMode[0].fireRate);
        
        if (_barbatos.actualWeaponModeIndex == WeaponMode.Secondary)
            _grenadeTimer = Normalize(1/_barbatos.so_Weapon.weaponMode[1].fireRate, 0, 1/_barbatos.so_Weapon.weaponMode[1].fireRate);
    }
    
    private void Update()
    {
        _shotgunTimer.DecreaseTimerIfPositive();
        _grenadeTimer.DecreaseTimerIfPositive();

        shotgunBar.material.SetFloat("_BarAmount", Mathf.Lerp(1, 0, _shotgunTimer));
        grenadeBar.material.SetFloat("_BarAmount", Mathf.Lerp(1, 0, _grenadeTimer));
    }

    private void CreateMaterialInstance(ref MeshRenderer mr) => mr.material = Instantiate(mr.material);
    
    // Method to normalize a value from range [X, Y] to range [0, 1]
    float Normalize(float value, float min, float max)
    {
        return (value - min) / (max - min);
    }
}
