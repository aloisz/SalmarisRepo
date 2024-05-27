using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;

public class AI_Smasher_Perimeter0 : MonoBehaviour
{
    [Header("--- Component ---")] 
    [SerializeField] protected AI_Smasher aiSmasher;
    
    // Cac Attack
    
    [Header("--- TransformPos ---")] 
    [SerializeField] protected Transform cacAttackPos;
    [SerializeField] protected Transform impulse;
    
    
    [Header("Properties")] 
    [SerializeField] [Range(0,20)] protected float cacAttackSphereRadius;
    [ReadOnly] [SerializeField] protected bool isCacAttacking = false;
    [SerializeField] protected float countDownCacMultiplier;
    [SerializeField] protected float damageAppliedPerimeter0;
    [SerializeField] protected float knockBackStrenght; 
    
    
    
    #region Attack

    public void Reset()
    {
        isCacAttacking = false;
        timeElapsedForCacAttack = 0;
    }

    private float timeElapsedForCacAttack = 0;
    public void HandlePerimeter0()
    {
        timeElapsedForCacAttack += Time.deltaTime * countDownCacMultiplier;
        if (timeElapsedForCacAttack > aiSmasher.perimeters[0].timeSpentInPerimeter)
        {
            timeElapsedForCacAttack = 0;
            aiSmasher.aiSmasherAnimator.ChangeState(aiSmasher.aiSmasherAnimator.ATTACK);
            CacAttackPerimeter0();
        }
        else isCacAttacking = false;
    }


    private IDamage pl;
    private void CacAttackPerimeter0()
    {
        isCacAttacking = true;
        Collider[] colliders = Physics.OverlapSphere(cacAttackPos.position, cacAttackSphereRadius, aiSmasher.targetMask);

        foreach (var obj in colliders)
        {
            if (obj.transform.CompareTag("Player"))
            {
                if (obj.transform.TryGetComponent(out pl))
                {
                    pl.Hit(damageAppliedPerimeter0);
                }

                var dir = (obj.transform.position - impulse.position).normalized;
                aiSmasher.ApplyKnockBack(knockBackStrenght, dir);
            }
        }
    }
    #endregion


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        DebugCacAttack();
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
