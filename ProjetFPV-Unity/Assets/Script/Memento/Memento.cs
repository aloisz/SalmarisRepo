using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


[Serializable]
public class Memento
{
    
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
    
    // Constructor
    public Memento(Transform playerTransform, PlayerHealth playerHealth)
    {
        SetPlayerPosition(playerTransform.position);
        SetPlayerDirection(playerTransform.rotation);
        
        SetPlayerHealth(playerHealth.Health);
        SetPlayerShield(playerHealth.Shield);
    }
    
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
}

