using System;
using System.Collections;
using System.Collections.Generic;
using AI;
using Player;
using UnityEditor;
using UnityEngine;
using NaughtyAttributes;

public class AI_Smasher : AI_Pawn
{
    [Header("----- AI_Smasher -----")] 
    [SerializeField] protected SmasherMobState smasherMobState;
    
    [Header("--- Perimeter ---")] [SerializeField]
    protected List<Perimeters> perimeters;

    [Space]
    // Cac Attack
    [Foldout("Cac Attack")]
    [Header("--- TransformPos ---")] 
    [SerializeField] protected Transform cacAttackPos;
    [Foldout("Cac Attack")][SerializeField] protected Transform impulse;
    
    [Foldout("Cac Attack")]
    [Header("CacAttack Properties")] 
    [SerializeField] [Range(0,20)] protected float cacAttackSphereRadius;
   
    [Foldout("Cac Attack")]
    [ReadOnly] [SerializeField] protected bool isCacAttacking = false;

    [Foldout("Cac Attack")] [SerializeField]
    protected float countDownCacMultiplier;
    
    [Foldout("Cac Attack")]
    [SerializeField] protected LayerMask cacAttackLayer;
    
    protected enum SmasherMobState
    {
        Perimeter_0,
        Perimeter_1,
        Perimeter_2,
        Perimeter_3
    }
    
    protected SmasherMobState ChangeState(SmasherMobState state)
    {
        return this.smasherMobState = state;
    }
        
    protected override void PawnBehavior()
    {
        base.PawnBehavior();
        switch (smasherMobState)
        {
            case SmasherMobState.Perimeter_0:
                CacManagement();
                break;
            case SmasherMobState.Perimeter_1:
                break;
            case SmasherMobState.Perimeter_2:
                break;
            case SmasherMobState.Perimeter_3:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        CheckDistance();
    }



    #region Attack

    private float timeElapsedForCacAttack = 0;
    private void CacManagement()
    {
        timeElapsedForCacAttack += Time.deltaTime * countDownCacMultiplier;
        if (timeElapsedForCacAttack > perimeters[0].timeSpentInPerimeter)
        {
            timeElapsedForCacAttack = 0;
            CacAttack();
        }
        else isCacAttacking = false;
    }
    
    private void CacAttack()
    {
        isCacAttacking = true;
        Collider[] colliders = Physics.OverlapSphere(cacAttackPos.position, cacAttackSphereRadius, cacAttackLayer);

        foreach (var obj in colliders)
        {
            if (obj.transform.CompareTag("Player"))
            {
                ApplyDamage(50);

                var dir = (obj.transform.position - impulse.position).normalized;
                ApplyKnockBack(1500, dir);
            }
        }
    }
    #endregion
    
    private void ApplyDamage(float damage)
    {
        
    }

    private void ApplyKnockBack(float strenght, Vector3 dir)
    {
        PlayerController.Instance._rb.AddForce(dir * strenght);
    }
    
    /// <summary>
    /// Adapt the agent speed if the player is too far away
    /// </summary>
    /// <returns></returns>
    protected virtual void CheckDistance()
    {
        switch (Vector3.Distance(PlayerController.Instance.transform.position, transform.position))
        {
            case var value when value < perimeters[0].distToEnemy:
                ChangeState(SmasherMobState.Perimeter_0);
                break;
            case var value when value < perimeters[1].distToEnemy:
                ChangeState(SmasherMobState.Perimeter_1);
                break;
            case var value when value < perimeters[2].distToEnemy:
                ChangeState(SmasherMobState.Perimeter_2);
                break;
            case var value when value < perimeters[3].distToEnemy:
                ChangeState(SmasherMobState.Perimeter_3);
                break;
        }
    }
    
    // Debug -------------------------
    #if UNITY_EDITOR
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        var tr = transform;
        var pos = tr.position;
        
        Handles.color = Color.green;
        Handles.DrawLine(pos, pos + transform.forward * 10, 2);
        
        DebugDistance(tr, pos);
        DebugCacAttack();
        
        if(Application.isPlaying)
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.red;
            style.fontSize = 25;
            Handles.Label(pos + new Vector3(0,5,0), $"State {smasherMobState}", style);
        }
    }

    private void DebugDistance(Transform tr, Vector3 pos)
    {
        for (int i = 0; i < perimeters.Count; i++)
        {
            Color32 color = new Color32();
            switch (i)
            {
                case 0:
                    color = new Color32(0, 125, 255, 50); 
                    break;
                case 1:
                    color = new Color32(0, 125, 255, 30); 
                    break;
                case 2:
                    color = new Color32(0, 125, 255, 20); 
                    break;
                case 3:
                    color = new Color32(0, 125, 255, 10); 
                    break;
            }
            Handles.color = color;
            Handles.DrawSolidDisc(pos, tr.up, perimeters[i].distToEnemy);
        }
    }

    private void DebugCacAttack()
    {
        if(!isCacAttacking) return;
        Handles.color = Color.yellow;
        Handles.DrawWireArc(cacAttackPos.position, transform.up, transform.right, 360, cacAttackSphereRadius, 3);
        Handles.DrawWireArc(cacAttackPos.position, transform.right, transform.up, 360, cacAttackSphereRadius, 3);
        Handles.DrawWireArc(cacAttackPos.position, transform.forward, transform.right, 360, cacAttackSphereRadius, 3);
    }
    #endif
}

[System.Serializable]
public class Perimeters
{
    [Range(0, 120)] public float distToEnemy;
    [Range(0, 120)] public float timeSpentInPerimeter;
}
