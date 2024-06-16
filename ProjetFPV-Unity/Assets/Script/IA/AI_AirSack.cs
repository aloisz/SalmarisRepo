using System;
using System.Collections;
using System.Collections.Generic;
using AI;
using MyAudio;
using Player;
using UnityEditor;
using UnityEngine;
using Weapon;

public class AI_AirSack : AI_Pawn
{
    [Header("--- AI_AirSack ---")] 
    [SerializeField] protected AirSackMobState airSackMobState; 
    
    [Space]
    [SerializeField] protected Transform shootingPos;
    protected WeaponManager weapon;
    protected bool isShooting;
    
    [Space]
    [SerializeField] protected float agentGetAwayRadius = 2;
    [SerializeField] protected float agentSpeedGettingAway = 50;
    [SerializeField] protected float agentShootingRadius = 5;
    
    
    // Component
    protected internal AI_Animator_AirSack animatorAirSack;
    
    protected enum AirSackMobState
    {
        Idle,
        MovingTowardPlayer,
        RunningAway
    }

    protected override void Awake()
    {
        base.Awake();
        weapon = GetComponent<WeaponManager>();
        animatorAirSack = GetComponent<AI_Animator_AirSack>();
    }

    public override void ResetAgent(bool doAudio)
    {
        base.ResetAgent(doAudio);
        if(doAudio) AudioManager.Instance.SpawnAudio3D(transform.position, SfxType.SFX, 18, 1, 0, 1);
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
                isShooting = true;
                weapon.ShootingAction();
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
    
    protected override void SetTarget (Vector3 targetToFollow)
    {
        switch (airSackMobState)
        {
            case AirSackMobState.Idle:
                navMeshAgent.SetDestination(targetToFollow);
                break;
            case AirSackMobState.MovingTowardPlayer:
                navMeshAgent.SetDestination(targetToFollow);
                break;
            case AirSackMobState.RunningAway:
                navMeshAgent.SetDestination(Player.PlayerController.Instance.transform.position + transform.position);
                break;
        }
    }
    
    public override void IsPhysicNavMesh(bool condition)
    {
        navMeshAgent.enabled = condition;
    }
    
    public override void DestroyLogic()
    {
        AudioManager.Instance.SpawnAudio3D(transform.position, SfxType.SFX, 17, 1, 0, 1);
        Pooling.Instance.DelayedDePop(so_IA.poolingName, gameObject, 2f);
        animatorAirSack.ChangeState(animatorAirSack.DEATH,.2f);
    }
    
    public override void Hit(float damageInflicted)
    {
        base.Hit(damageInflicted);
    }
    
    protected override void HitAudio()
    {
        AudioManager.Instance.SpawnAudio3D(transform.position, SfxType.SFX, 16, 1, 0, 1,1, 0,
            AudioRolloffMode.Linear, 5,100);
    }
    
    
    
    // Debug -------------------------
    #if UNITY_EDITOR
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        if(Application.isPlaying)
        {
            Handles.Label(transform.position + new Vector3(0,1,0), $"airSackMobState : {airSackMobState}");
            Handles.Label(transform.position + new Vector3(0,0,0), $"is shooting : {isShooting}");
            Handles.Label(transform.position + new Vector3(0,2,0), $"Actual Health : {actualPawnHealth}");
        }
        
            
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
