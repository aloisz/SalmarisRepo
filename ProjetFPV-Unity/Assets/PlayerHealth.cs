using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerHealth : GenericSingletonClass<PlayerHealth>
{
    [SerializeField] private PlayerScriptable playerScriptable;

    private float _health;
    public float Health
    {
        get => _health;
        private set => _health = Mathf.Clamp(value, 0f, maxHealth);
    }
    
    private float _shield;
    public float Shield
    {
        get => _shield;
        private set => _shield = Mathf.Clamp(value, 0f, maxShield);
    }

    public Action onHit;

    public float maxHealth;
    public float maxShield;
    
    // Start is called before the first frame update
    void Start()
    {
        InitValues();
    }

    private void InitValues()
    {
        maxHealth = playerScriptable.maxPlayerHealth;
        maxShield = playerScriptable.maxPlayerShield;

        Health = maxHealth;
        Shield = maxShield;
    }

    public void ApplyDamage(float amount)
    {
        if (Shield > 0)
        {
            if (amount >= Shield)
            {
                // Damage exceeds shield, consume shield first
                amount -= Shield;
                Shield = 0;
            }
            else
            {
                // Damage does not exceed shield, only deplete shield
                Shield -= amount;
                amount = 0;
            }
        }

        // Apply remaining damage to health
        Health -= amount;

        // Check if health drops below 0
        if (Health <= 0)
        {
            //Die();
        }
        
        onHit?.Invoke();
    }

    public void RestoreShield(float amount) => Shield += amount;
}
