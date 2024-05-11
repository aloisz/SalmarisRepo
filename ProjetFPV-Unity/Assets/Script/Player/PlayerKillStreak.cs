using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class PlayerKillStreak : GenericSingletonClass<PlayerKillStreak>
{
    [Header("Overall")]
    public float maxKillStreak = 100f;
    public bool isInRageMode;
    
    public float KillStreak
    {
        get => _killStreak;
        private set => _killStreak = Mathf.Clamp(value, 0f, maxKillStreak);
    }
    private float _killStreak;

    [Header("Multipliers")]
    public float staminaBoost;
    public float reloadBoost;
    public float fireRateBoost;
    public float speedBoost;
    public float healPerKill;
    public ShieldPerEnemy[] shieldPerEnemies;
    
    [Header("Value Increment")]
    [SerializeField] private float incrementKillStreakDmg = 10f;
    [SerializeField] private float incrementKillStreakKill = 10f;
    
    [Header("Value Decrement")]
    [SerializeField] private float decrementInactivityP = 10f;
    [SerializeField] private float decrementPlayerDmgP = 10f;
    
    [Header("Inactivity")]
    [SerializeField] private float inactivityTimeAllowed = 3f;
    
    [Header("Multipliers Values No Rage")]
    [SerializeField] private float staminaMaxValue = 2f;
    [SerializeField] private float reloadMaxValue = 2f;
    [SerializeField] private float fireRateMaxValue = 2f;
    [SerializeField] private float speedMaxValue = 2f;
    
    [Header("Multipliers Values Rage")]
    [SerializeField] private float staminaMaxValueR = 2f;
    [SerializeField] private float reloadMaxValueR = 2f;
    [SerializeField] private float fireRateMaxValueR = 2f;
    [SerializeField] private float speedMaxValueR = 2f;
    [SerializeField] private float healMaxValueR = 2f;
    
    [Header("Rage")]
    [SerializeField] private float rageBaseDuration = 3f;
    [SerializeField] private float refilePenaltyDuration = 3f;

    private float _inactivityTimer;
    private float _rageTimer;
    private float _rageTransitionPenaltyTimer;
    
    private bool _canRefileRage;

    private void Start()
    {
        PlayerHealth.Instance.onHit += () => DecrementKilLStreak(decrementPlayerDmgP);

        //StartCoroutine(nameof(DebugRoutine));
    }

    IEnumerator DebugRoutine()
    {
        if(!isInRageMode) IncrementKilLStreak(20f);
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(nameof(DebugRoutine));
    }

    public void NotifyDamageInflicted()
    {
        IncrementKilLStreak(incrementKillStreakDmg);
    }
    
    public void NotifyEnemyKilled(EnemyToSpawn.EnemyKeys key)
    {
        IncrementKilLStreak(incrementKillStreakKill);
        PlayerHealth.Instance.RestoreHealth(healPerKill);

        foreach (ShieldPerEnemy spe in shieldPerEnemies)
        {
            if (spe.key == key)
            {
                if(Random.value > 1-PlayerHealth.Instance.probabilityToGainShield) 
                    PlayerHealth.Instance.RestoreShield(spe.shieldValue);
            }
        }
    }

    private void IncrementKilLStreak(float amount)
    {
        if (!_canRefileRage) return;

        ResetInactivityTimer();
        
        KillStreak += isInRageMode ? amount / 3f : amount;
        if(KillStreak >= 100f) ResetRageTimer();
    }
    
    private void DecrementKilLStreak(float amount)
    {
        KillStreak -= amount;
    }

    private void CheckIfIsInRageMode() => isInRageMode = KillStreak > 0f && _rageTimer > 0f;
    
    private void ResetInactivityTimer() => _inactivityTimer = inactivityTimeAllowed;
    private void ResetRageTimer() => _rageTimer = rageBaseDuration;

    private void SetupBoostsNoRage()
    {
        staminaBoost = Mathf.Lerp(staminaBoost, Mathf.Lerp(1, staminaMaxValue, KillStreak / 100f), Time.deltaTime * 5f);
        reloadBoost = Mathf.Lerp(reloadBoost, Mathf.Lerp(1, reloadMaxValue, KillStreak / 100f), Time.deltaTime * 5f);
        fireRateBoost = Mathf.Lerp(fireRateBoost, Mathf.Lerp(1, fireRateMaxValue, KillStreak / 100f), Time.deltaTime * 5f);
        speedBoost = Mathf.Lerp(speedBoost, Mathf.Lerp(1, speedMaxValue, KillStreak / 100f), Time.deltaTime * 5f);

        healPerKill = 0f;
    }
    
    private void SetupBoostsRage()
    {
        staminaBoost = staminaMaxValueR;
        reloadBoost = reloadMaxValueR;
        fireRateBoost = fireRateMaxValueR;
        speedBoost = speedMaxValueR;
        healPerKill = healMaxValueR;
    }
    
    private void Update()
    {
        if (!isInRageMode && _canRefileRage)
        {
            _inactivityTimer.DecreaseTimerIfPositive();
        }
        else if(isInRageMode && _canRefileRage)
        {
            ResetInactivityTimer();
            
            _rageTimer.DecreaseTimerIfPositive();
            
            KillStreak = Mathf.Lerp(KillStreak, 0f, Time.deltaTime / _rageTimer);

            if (KillStreak <= 0f && isInRageMode && _canRefileRage)
            {
                _rageTransitionPenaltyTimer = refilePenaltyDuration;
                _rageTimer = 0f;
                KillStreak = 0f;
            }
            else _rageTransitionPenaltyTimer = 0f;
        }
        else
        {
            _rageTransitionPenaltyTimer.DecreaseTimerIfPositive();
        }

        _canRefileRage = _rageTransitionPenaltyTimer <= 0f;

        if(_inactivityTimer <= 0f) DecrementKilLStreak((decrementInactivityP * maxKillStreak / 100f) * Time.deltaTime);

        if(!isInRageMode) SetupBoostsNoRage();
        else SetupBoostsRage();
        
        CheckIfIsInRageMode();
    }
}

[Serializable]
public class ShieldPerEnemy
{
    public float shieldValue;
    public EnemyToSpawn.EnemyKeys key;
}
