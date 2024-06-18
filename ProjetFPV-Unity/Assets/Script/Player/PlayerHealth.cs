using System;
using System.Collections;
using System.Collections.Generic;
using AI;
using CameraBehavior;
using MyAudio;
using Player;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class PlayerHealth : GenericSingletonClass<PlayerHealth>, IDamage
{
    [SerializeField] private float explosionRadius, explosionForce;
    [SerializeField] private LayerMask explosionMask;
    
    [SerializeField] private AnimationCurve healthCamShakeCurve;

    public Vector3 lastEnemyPosition;
    
    [HideInInspector] public float _health;
    public float Health
    {
        get => _health;
        private set => _health = Mathf.Clamp(value, 0f, maxHealth);
    }
    
    [HideInInspector] public float _shield;
    public float Shield
    {
        get => _shield;
        private set => _shield = Mathf.Clamp(value, 0f, maxShield);
    }

    public Action onHit;
    public Action onDeath;

    public float maxHealth;
    public float maxShield;
    public float probabilityToGainShield;

    private float _timerRegenShield;
    
    // Start is called before the first frame update
    void Start()
    {
        InitValues();
        //onDeath += CareTaker.Instance.LoadGameState;
        onHit += () => _timerRegenShield = PlayerController.Instance.playerScriptable.shieldRegenCooldown;
    }

    private void InitValues()
    {
        maxHealth = PlayerController.Instance.playerScriptable.maxPlayerHealth;
        maxShield = PlayerController.Instance.playerScriptable.maxPlayerShield;

        Health = maxHealth;
        Shield = maxShield;
    }

    private void Update()
    {
        _timerRegenShield.DecreaseTimerIfPositive();
        if (_timerRegenShield <= 0f)
        {
            RestoreShield(PlayerController.Instance.playerScriptable.shieldPerSecondWhenRegen * Time.deltaTime);
        }
    }

    public void RestoreShield(float amount) => Shield += amount;
    public void RestoreHealth(float amount) => Health += amount;

    private void ShockwaveBreakShield()
    {
        Collider[] surroundingObj = Physics.OverlapSphere(transform.position, explosionRadius, explosionMask);
        
        foreach (Collider obj in surroundingObj)
        {
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
        if (!OptionsDDOL.Instance) return;
        if (OptionsDDOL.Instance.isInGodMod) return;
        ApplyDamage(damageInflicted);
    }

    private bool _alreadyPlayedNoLifeVoiceLine;
    
    private void ApplyDamage(float amount)
    {
        if (Shield > 0)
        {
            if (amount >= Shield)
            {
                //Audio
                AudioManager.Instance.SpawnAudio2D(transform.position, SfxType.SFX, 48, 1f,0,1,false);
                // Damage exceeds shield, consume shield first
                amount -= Shield;
                Shield = 0;
                ShockwaveBreakShield();

                VoicelineManager.Instance.CallFirstBrokenShieldVoiceLine();
            }
            else
            {
                //Audio
                AudioManager.Instance.SpawnAudio2D(transform.position, SfxType.SFX, 47, 1f,0,1,false);
                // Damage does not exceed shield, only deplete shield
                Shield -= amount;
                amount = 0;
            }
        }
        else
        {
            //Audio
            AudioManager.Instance.SpawnAudio2D(transform.position, SfxType.SFX, 48, 1f,0,1,false);
        }

        // Apply remaining damage to health
        Health -= amount;

        if (Health < 30f)
        {
            if (!_alreadyPlayedNoLifeVoiceLine)
            {
                VoicelineManager.Instance.CallLowLifeVoiceLine();
                _alreadyPlayedNoLifeVoiceLine = true;
            }
        }
        else
        {
            _alreadyPlayedNoLifeVoiceLine = false;
        }

        // Check if health drops below 0
        if (Health <= 0)
        {
            Death();
            
            VoicelineManager.Instance.CallFirstDeathVoiceLine(false);
            MusicManager.Instance.ChangeMusicPlayed(Music.Ambiance, 0.2f, 0.25f);
            return;
        }
        
        onHit?.Invoke();
    }

    public void DeathFromHole()
    {
        Death();
        
        if(!VoicelineManager.Instance._alreadyFirstDied) VoicelineManager.Instance.CallFirstDeathVoiceLine(true);
        else VoicelineManager.Instance.CallHoleDeathVoiceLine();
    }
    
    private int[] randomSound = {33,34,35,36};
    [ContextMenu("Death")]
    public void Death()
    {
        onDeath.Invoke();

        Time.timeScale = 0f;
        
        HUD.Instance.Death();
        
        Director.Instance.StopAllCoroutines();
        StartCoroutine(nameof(DeathRoutine));
    }

    private IEnumerator DeathRoutine()
    {
        MusicManager.Instance.ChangeMusicPlayed(Music.Ambiance, 0.2f, 0f);
        PlayerInputs.Instance.EnablePlayerInputs(false);
        
        VoicelineManager.Instance.PlayPriorityVoiceLine(53);
        
        var randomNumber = Random.Range(randomSound[0], randomSound[3]);
        AudioManager.Instance.SpawnAudio2D(transform.position, SfxType.SFX, randomNumber, 1,0,1);

        yield return new WaitForSecondsRealtime(0.75f);
        
        CareTaker.Instance.LoadGameState();

        yield return new WaitForSecondsRealtime(1f);
        
        Time.timeScale = 1f;
        
        PlayerInputs.Instance.EnablePlayerInputs(true);
        
        MusicManager.Instance.ChangeMusicPlayed(Music.Ambiance, 0.2f, 0.25f);
    }
}
