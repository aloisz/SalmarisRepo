using System;
using System.Collections;
using System.Collections.Generic;
using AI;
using Player;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerHealth : GenericSingletonClass<PlayerHealth>, IDamage
{
    [SerializeField] private float explosionRadius, explosionForce;
    [SerializeField] private LayerMask explosionMask;

    public Vector3 lastEnemyPosition;
    
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
        maxHealth = PlayerController.Instance.playerScriptable.maxPlayerHealth;
        maxShield = PlayerController.Instance.playerScriptable.maxPlayerShield;

        Health = maxHealth;
        Shield = maxShield;
    }

    public void RestoreShield(float amount) => Shield += amount;

    private void ShockwaveBreakShield()
    {
        Collider[] surroundingObj = Physics.OverlapSphere(transform.position, explosionRadius, explosionMask);
        
        foreach (Collider obj in surroundingObj)
        {
            Debug.Log("Booom");
            
            if (obj.GetComponent<AI_Pawn>() is not null)
            {
                var enemy = obj.GetComponent<AI_Pawn>();
                enemy.DisableAgent();
                
                var rb = obj.GetComponent<Rigidbody>();
                var direction = ((obj.transform.position - transform.position) - Vector3.up * 0.75f).normalized;
                
                rb.AddForce(direction * explosionForce, ForceMode.Impulse);
            }
        }
    }

    public void Hit(float damageInflicted)
    {
        ApplyDamage(damageInflicted);
    }
    private void ApplyDamage(float amount)
    {
        if (Shield > 0)
        {
            if (amount >= Shield)
            {
                // Damage exceeds shield, consume shield first
                amount -= Shield;
                Shield = 0;
                ShockwaveBreakShield();
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
}
