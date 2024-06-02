using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
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
    
    
    // Constructor
    public Memento(Transform playerTransform, PlayerHealth playerHealth, WeaponManager barbatos)
    {
        // PLAYER
        SetPlayerPosition(playerTransform.position);
        SetPlayerDirection(playerTransform.rotation);
        
        SetPlayerHealth(playerHealth.Health);
        SetPlayerShield(playerHealth.Shield);
        
        // GUN
        SetSOWeaponMode(0, barbatos.so_Weapon.weaponMode[0]);
        SetSOWeaponMode(1, barbatos.so_Weapon.weaponMode[1]);
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
    private void SetPlayerDirection(Quaternion rotation)
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
}

