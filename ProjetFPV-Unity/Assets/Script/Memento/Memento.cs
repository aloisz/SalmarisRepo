using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Weapon;


[Serializable]
public class Memento
{
    
    //--------------------------------------
    // PLAYER
    // Player Position
    private float playerPosX;
    private float playerPosY;
    private float playerPosZ;
    
    // Player Rotation
    private float playerRotX;
    private float playerRotY;
    private float playerRotZ;
    private float playerRotW;
    
    // Player Health
    private float playerHealth;
    private float playerShield;
    
    //--------------------------------------
    // GUN
    private SO_WeaponMode[] soWeaponMode = new SO_WeaponMode[2];
    
    //--------------------------------------
    // DIRECTOR
    
    private int totalIntensityValue;
    private int totalIntensityValueLevel;
    private int numberOfDeath;
    private int currentArenaIndex;
    private int currentWaveIndex;
    private int playerOverPerfomAmount;
    private int _currentRemainingEnemies;
    private int _arenaAmount;
    
    private float _currentWaveIntensity;
    private float _playerPerformance;
    private float _dynamicNextWaveValue;
    private float _timerToCheckPlayerPerformance;
    private float _lastKilledEnemiesValue;
    
    private bool _currentArenaFinished;
    private bool _isInAArena;
    private bool _isInAWave;
    private bool _hasFinishSpawningEnemies;
    private bool _hasStartedWave;

    private List<bool> keysPickedUp = new List<bool>();
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="playerTransform"></param>
    /// <param name="playerHealth"></param>
    /// <param name="barbatos"></param>
    public Memento(Transform playerTransform, PlayerHealth playerHealth, WeaponManager barbatos, Director director, List<bool> keysPickedUp)
    {
        // PLAYER
        SetPlayerPosition(playerTransform.position);
        SetPlayerRotation(playerTransform.rotation);
        
        SetPlayerHealth(playerHealth.Health);
        SetPlayerShield(playerHealth.Shield);
        
        // GUN
        SetSOWeaponMode(0, barbatos.so_Weapon.weaponMode[0]);
        SetSOWeaponMode(1, barbatos.so_Weapon.weaponMode[1]);
        
        // DIRECTOR
        SetTotalIntensityValue(director.totalIntensityValue);
        SetTotalIntensityValueLevel((director.totalIntensityValueLevel));
        SetNumberOfDeath(director.numberOfDeath);
        SetCurrentArenaIndex(director.currentArenaIndex);
        SetCurrentWaveIndex(director.currentWaveIndex);
        SetPlayerOverPerfomAmount(director.playerOverPerfomAmount);
        SetCurrentRemainingEnemies(director._currentRemainingEnemies);
        SetArenaAmount(director._arenaAmount);
        
        SetCurrentWaveIntensity(director._currentWaveIntensity);
        SetPlayerPerformance(director._playerPerformance);
        SetDynamicNextWaveValue(director._dynamicNextWaveValue);
        SetTimerToCheckPlayerPerformance(director._timerToCheckPlayerPerformance);
        SetLastKilledEnemiesValue(director._lastKilledEnemiesValue);
        
        SetCurrentArenaFinished(director._currentArenaFinished);
        SetIsInAArena(director._isInAArena);
        SetIsInAWave(director._isInAWave);
        SetHasFinishSpawningEnemies(director._hasFinishSpawningEnemies);
        SetHasStartedWave(director._hasStartedWave);

        this.keysPickedUp = keysPickedUp;
    }

    #region PLAYER ----------------

    // Player Position ________________
    private void SetPlayerPosition(Vector3 position)
    {
        this.playerPosX = position.x;
        this.playerPosY = position.y;
        this.playerPosZ = position.z;
    }
    public Vector3 GetPlayerPosition()
    {
        return new Vector3(playerPosX, playerPosY, playerPosZ);
    }

    
    // Player Rotation ________________
    private void SetPlayerRotation(Quaternion rotation)
    {
        this.playerRotX = rotation.x;
        this.playerRotY = rotation.y;
        this.playerRotZ = rotation.z;
        this.playerRotW = rotation.w;
    }
    public Quaternion GetPlayerRotation()
    {
        return new Quaternion(playerRotX, playerRotY, playerRotZ, playerRotW);
    }
    
    // Player Health ________________
    private void SetPlayerHealth(float health)
    {
        this.playerHealth = health;
    }
    public float GetPlayerHealth()
    {
        return playerHealth;
    }
    
    private void SetPlayerShield(float shield)
    {
        this.playerShield = shield;
    }
    public float GetPlayerShield()
    {
        return playerShield;
    }

    #endregion

    #region GUN ----------------

    // Gun Actual SO  ________________

    private void SetSOWeaponMode(int modeIndex, SO_WeaponMode mode)
    {
        this.soWeaponMode[modeIndex] = mode;
    }
    
    public SO_WeaponMode GetSOWeaponMode(int modeIndex)
    {
        return this.soWeaponMode[modeIndex];
    }

    #endregion

    #region DIRECTOR -------------
    
    // INT SECTION -------------------------------
    private void SetTotalIntensityValue(int totalIntensityValue)
    {
        this.totalIntensityValue = totalIntensityValue;
    }
    public int GetTotalIntensityValue() => this.totalIntensityValue;
    
    private void SetTotalIntensityValueLevel(int totalIntensityValueLevel)
    {
        this.totalIntensityValueLevel = totalIntensityValueLevel;
    }
    public int GetTotalIntensityValueLevel() => this.totalIntensityValueLevel;

    private void SetNumberOfDeath(int numberOfDeath)
    {
        this.numberOfDeath = numberOfDeath;
    }
    public int GetNumberOfDeath() => this.numberOfDeath;

    private void SetCurrentArenaIndex(int currentArenaIndex)
    {
        this.currentArenaIndex = currentArenaIndex;
    }
    public int GetCurrentArenaIndex() => this.currentArenaIndex;

    private void SetCurrentWaveIndex(int currentWaveIndex)
    {
        this.currentWaveIndex = currentWaveIndex;
    }
    public int GetCurrentWaveIndex() => this.currentWaveIndex;

    private void SetPlayerOverPerfomAmount(int playerOverPerfomAmount)
    {
        this.playerOverPerfomAmount = playerOverPerfomAmount;
    }
    public int GetPlayerOverPerfomAmount() => this.playerOverPerfomAmount;

    private void SetCurrentRemainingEnemies(int _currentRemainingEnemies)
    {
        this._currentRemainingEnemies = _currentRemainingEnemies;
    }
    public int GetCurrentRemainingEnemies() => this._currentRemainingEnemies;

    private void SetArenaAmount(int _arenaAmount)
    {
        this._arenaAmount = _arenaAmount;
    }
    public int GetArenaAmount() => this._arenaAmount;
    
    // FLOAT SECTION -------------------------------
    private void SetCurrentWaveIntensity(float _currentWaveIntensity)
    {
        this._currentWaveIntensity = _currentWaveIntensity;
    }
    public float GetCurrentWaveIntensity() => this._currentWaveIntensity;

    private void SetPlayerPerformance(float _playerPerformance)
    {
        this._playerPerformance = _playerPerformance;
    }
    public float GetPlayerPerformance() => this._playerPerformance;

    private void SetDynamicNextWaveValue(float _dynamicNextWaveValue)
    {
        this._dynamicNextWaveValue = _dynamicNextWaveValue;
    }
    public float GetDynamicNextWaveValue() => _dynamicNextWaveValue;

    private void SetTimerToCheckPlayerPerformance(float _timerToCheckPlayerPerformance)
    {
        this._timerToCheckPlayerPerformance = _timerToCheckPlayerPerformance;
    }
    public float GetTimerToCheckPlayerPerformance() => _timerToCheckPlayerPerformance;

    private void SetLastKilledEnemiesValue(float _lastKilledEnemiesValue)
    {
        this._lastKilledEnemiesValue = _lastKilledEnemiesValue;
    }
    public float GetLastKilledEnemiesValue() => _lastKilledEnemiesValue;

    // Bool SECTION -------------------------------
    private void SetCurrentArenaFinished(bool _currentArenaFinished)
    {
        this._currentArenaFinished = _currentArenaFinished;
    }
    public bool GetCurrentArenaFinished() => _currentArenaFinished;

    private void SetIsInAArena(bool _isInAArena)
    {
        this._isInAArena = _isInAArena;
    }
    public bool GetIsInAArena() => _isInAArena;

    private void SetIsInAWave(bool _isInAWave)
    {
        this._isInAWave = _isInAWave;
    }
    public bool GetIsInAWave() => _isInAWave;

    private void SetHasFinishSpawningEnemies(bool _hasFinishSpawningEnemies)
    {
        this._hasFinishSpawningEnemies = _hasFinishSpawningEnemies;
    }
    public bool GetHasFinishSpawningEnemies() => _hasFinishSpawningEnemies;

    private void SetHasStartedWave(bool _hasStartedWave)
    {
        this._hasStartedWave = _hasStartedWave;
    }
    public bool GetHasStartedWave() => _hasStartedWave;

    #endregion
    
    #region DOORS ----------------

    public List<bool> GetKeysPickedUp() => keysPickedUp;

    #endregion
}

