using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using Player;
using UnityEditor;
using UnityEditor.Searcher;
using UnityEngine;


namespace AI
{
    public class AI_Smasher_Perimeter1 : MonoBehaviour
    {
        [Header("--- Component ---")] 
        [SerializeField] protected AI_Smasher aiSmasher;
        
        [Header("--- TransformPos ---")] 
        [SerializeField] protected Transform cacAttackPos;
        [SerializeField] protected Transform impulse;
        
        
        [Header("Properties")] 
        [SerializeField] [Range(0,20)] protected float attackSphereRadius;
        [ReadOnly] [SerializeField] protected bool isAttacking = false;
        [SerializeField] [Range(0,20)]protected float timeWaitBeforeDash;
        [SerializeField] protected float countDownCacMultiplier;
        [SerializeField] protected float damageApplied;
        [SerializeField] protected float knockBackStrenght;

        [Space] [SerializeField] private float timeBeforeJumping;
        [SerializeField] protected AnimationCurve timeToDash;
        
        
        
        [Header("Debugging")] 
        [SerializeField] private bool enableDebugging;


        private void Update()
        {
            if (aiSmasher.pawnState != PawnState.Enable) return;
            
            if(isPreparingDash) PreparingDash();
            if (isAttacking)
            {
                Attack();
                aiSmasher.IsPhysicNavMesh(false); // disable pawn Physics 
            }
                
            if (isAttacking || isPreparingDash)
            {
                transform.DOLookAt(PlayerController.Instance.transform.position + Vector3.up, 0.2f, AxisConstraint.Y);
            }
            HandleHasLanded();
        }

        #region Attack

        internal float timeElapsedInPerimeter = 0;
        internal bool isInPerimeter;
        public void HandlePerimeter1()
        {
            timeElapsedInPerimeter += Time.deltaTime * countDownCacMultiplier;
            if (timeElapsedInPerimeter > aiSmasher.perimeters[1].timeSpentInPerimeter && !isPreparingDash && !isAttacking)
            {
                timeElapsedInPerimeter = 0;
                isPreparingDash = true;
            }
        }

        private bool isPreparingDash;
        private float timeToPrepareDash = 0;
        private void PreparingDash()
        {
            aiSmasher.navMeshAgent.speed = 0;

            if (Vector3.Distance(PlayerController.Instance.transform.position, transform.position) >
                aiSmasher.perimeters[2].distToEnemy) isPreparingDash = false;
            
            timeElapsedInPerimeter += Time.deltaTime * 1;
            if (timeElapsedInPerimeter > timeWaitBeforeDash)
            {
                isPreparingDash = false;
                isAttacking = true;
                timeElapsedInPerimeter = 0;

                StartCoroutine(BeginDash(timeBeforeJumping));
            }
        }

        IEnumerator BeginDash(float delayedTime)
        {
            float distToPlayer = Vector3.Distance(PlayerController.Instance.transform.position, impulse.position);
            float time = timeToDash.Evaluate(distToPlayer);
            landingPos = PlayerController.Instance.transform.position;
            
            yield return new WaitForSeconds(delayedTime);
            DashInPlayerDir(time, landingPos);
        }

        
        private Vector3 landingPos;
        private bool hasLanded = false;
        private void DashInPlayerDir(float time, Vector3 landingPos)
        {
            hasLanded = false;
            aiSmasher.transform.DOJump(landingPos, 1, 1, time).OnComplete((() =>
            {
                hasLanded = true;
                aiSmasher.IsPhysicNavMesh(true);
            }));
        }

        private float timeSinceLanded;
        private float maxTimeSinceLanded = 1f;
        private void HandleHasLanded()
        {
            if(!hasLanded) return;
            timeSinceLanded += Time.deltaTime * 1;
            if (!(timeSinceLanded > maxTimeSinceLanded)) return;
            timeSinceLanded = 0;
            hasLanded = false;
            isAttacking = false;
            aiSmasher.GetPawnPersonnalInformation();
        }
        
        
        private void Attack()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, attackSphereRadius, aiSmasher.targetMask);

            foreach (var obj in colliders)
            {
                if (obj.transform.CompareTag("Player"))
                {
                    PlayerHealth.Instance.ApplyDamage(damageApplied);
                    
                    var dir = (obj.transform.position - impulse.position).normalized;
                    aiSmasher.ApplyKnockBack(knockBackStrenght, dir);

                    StartCoroutine(EndAttack());
                }
            }
        }

        IEnumerator EndAttack()
        {
            yield return null;
            isAttacking = false;
            aiSmasher.GetPawnPersonnalInformation();
        }
        #endregion


        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if(!enableDebugging) return;
            DebugAttack();
        }
        
        private void DebugAttack()
        {
            if (isPreparingDash)
            {
                Handles.color = Color.magenta;
                Handles.DrawLine(transform.position, PlayerController.Instance.transform.position, 3);
                Handles.DrawSolidDisc(PlayerController.Instance.transform.position, transform.up, 2);
            }
            
            if(!isAttacking) return;
            Handles.color = Color.red;
            Handles.DrawSolidDisc(landingPos, transform.up, 2);
            
            Handles.color = Color.green;
            Handles.DrawWireArc(cacAttackPos.position, transform.up, transform.right, 360, attackSphereRadius, 3);
            Handles.DrawWireArc(cacAttackPos.position, transform.right, transform.up, 360, attackSphereRadius, 3);
            Handles.DrawWireArc(cacAttackPos.position, transform.forward, transform.right, 360, attackSphereRadius, 3);
        }
        #endif
    }
}

