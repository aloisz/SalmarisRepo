using System;
using System.Collections;
using System.Collections.Generic;
using AI;
using Player;
using UnityEditor;
using UnityEngine;
using Weapon;

public class AI_AirSack : AI_Pawn
{
    [Header("--- AI_TrashMob ---")] 
    [SerializeField] protected AirSackMobState airSackMobState; 
    
    [Space]
    [SerializeField] protected Transform shootingPos;
    protected WeaponManager weapon;
    protected bool isShooting;
    
    [Space]
    [SerializeField] protected float agentGetAwayRadius = 2;
    [SerializeField] protected float agentSpeedGettingAway = 50;
    [SerializeField] protected float agentShootingRadius = 5;
    
    protected enum AirSackMobState
    {
        Idle,
        MovingTowardPlayer,
        RunningAway
    }

    protected override void Start()
    {
        base.Start();
        weapon = GetComponent<WeaponManager>();
    }
    
    protected AirSackMobState ChangeState(AirSackMobState state)
    {
        return this.airSackMobState = state;
    }

    protected override void PawnBehavior()
    {
        base.PawnBehavior();
        switch (airSackMobState)
        {
            case AirSackMobState.Idle:
                isShooting = false;
                break;
            case AirSackMobState.MovingTowardPlayer:
                isShooting = true;
                weapon.ShootingAction();
                break;
            case AirSackMobState.RunningAway:
                isShooting = false;
                break;
        }
        CheckDistance();
    }

    /// <summary>
    /// Adapt the agent speed if the player is too far away
    /// </summary>
    /// <returns></returns>
    private Vector3 hitPoint;
    protected void CheckDistance()
    {
        RaycastHit hit;
        if (!Physics.Raycast(transform.position, Vector3.down, out hit, 1000)) return;
        hitPoint = hit.point;
        if (Vector3.Distance(PlayerController.Instance.transform.position, transform.position) > so_IA.visionDetectorRadius)
        {
            ChangeState(AirSackMobState.Idle);
        }
        else
        {
            if (Vector3.Distance(PlayerController.Instance.transform.position, hitPoint) > agentGetAwayRadius)
            {
                ChangeState(AirSackMobState.MovingTowardPlayer);
            }
            else
            {
                ChangeState(AirSackMobState.RunningAway);
            }
        }
        
    }
    
    
    protected override void DestroyLogic()
    {
        //TODO : Implement Pooling Depop 
        Destroy(gameObject);
    }
    
    
    
    // Debug -------------------------
    #if UNITY_EDITOR
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        if(!Application.isPlaying) return;
        Handles.Label(transform.position + new Vector3(0,1,0), $"airSackMobState : {airSackMobState}");
        Handles.Label(transform.position + new Vector3(0,0,0), $"is shooting : {isShooting}");
            
        var tr = transform;
        var pos = tr.position;
        
        // display a color disc 
        var color = new Color32(0, 125, 255, 10);
        Handles.color = color;
        Handles.DrawSolidDisc(hitPoint, tr.up, agentShootingRadius);
            
        var color2 = new Color32(255, 125, 255, 20);
        Handles.color = color2;
        Handles.DrawSolidDisc(hitPoint, tr.up, agentGetAwayRadius);
    }
    #endif
}
