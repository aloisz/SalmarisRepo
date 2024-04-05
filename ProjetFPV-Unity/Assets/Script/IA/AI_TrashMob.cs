using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Player;
using UnityEditor;
using UnityEngine;


namespace AI
{
    public class AI_TrashMob : AI_Pawn
    {
        [Header("--- AI_TrashMob ---")] 
        [SerializeField] protected TrashMobState trashMobState;
        [Space]
        [SerializeField] protected float agentSpeedRadius = 2;
        [SerializeField] protected float agentSpeedWhenIsToFar = 50;
        
        [Space]
        [SerializeField] protected float agentDashRadius = 2;
        [SerializeField] protected float agentDashSpeed = 500;
        [Tooltip("Each tick count increase, when matching value attack will be performed")] [SerializeField] protected float countBeforeAttack = 10;
        private int actualCountBeforeAttack = 0;
        private bool isPerformingAttack;
        
        protected enum TrashMobState
        {
            Idle,
            Moving,
            AttackingCloseRange
        }

        protected TrashMobState ChangeState(TrashMobState state)
        {
            return this.trashMobState = state;
        }
        
        protected override void PawnBehavior()
        {
            base.PawnBehavior();
            switch (trashMobState)
            {
                case TrashMobState.Idle:
                    break;
                case TrashMobState.Moving:
                    CheckDistance();
                    break;
                case TrashMobState.AttackingCloseRange:
                    CacAttack();
                    if(isInDashAttackCoroutine) return;
                    StartCoroutine(DashAttack());
                    break;
            }
        }
        
        
        /// <summary>
        /// Adapt the agent speed if the player is too far away
        /// </summary>
        /// <returns></returns>
        protected virtual void CheckDistance()
        {
            if (Vector3.Distance(PlayerController.Instance.transform.position, transform.position) > agentSpeedRadius)
            {
                navMeshAgent.speed = agentSpeedWhenIsToFar;
            }
            else
            {
                if (Vector3.Distance(PlayerController.Instance.transform.position, transform.position) > agentDashRadius)
                {
                    navMeshAgent.speed = so_IA.walkingSpeed;
                }
                else
                {
                    actualCountBeforeAttack++;
                    if (actualCountBeforeAttack >= countBeforeAttack)
                    {
                        ChangeState(TrashMobState.AttackingCloseRange);
                    }
                }
            }
        }

        private bool isInDashAttackCoroutine;
        private IEnumerator DashAttack()
        {
            isInDashAttackCoroutine = true;
            navMeshAgent.speed = 0;
            
            yield return new WaitForSeconds(1f);
            IsPhysicNavMesh(false);
            isPerformingAttack = true;
            
            yield return new WaitForSeconds(2);
            GetPawnPersonnalInformation();
            ChangeState(TrashMobState.Moving);
            actualCountBeforeAttack = 0;
            isInDashAttackCoroutine = false;
        }

        RaycastHit hit;
        bool hasHit = false;
        private void CacAttack()
        {
            hasHit = Physics.BoxCast(transform.position, transform.localScale*0.5f, 
                transform.forward, out hit, transform.rotation, 30, targetMask);
            if (hasHit)
            {
                Debug.Log("Hit : " + hit.transform.name);
            }
        }

        public override void DisableAgent()
        {
            base.DisableAgent();
            StopAllCoroutines();
            StartCoroutine(DisableAgentCorountine());
        }
        
        private IEnumerator DisableAgentCorountine()
        {
            yield return new WaitForSeconds(2);
            IsPhysicNavMesh(true);
        }

        protected void FixedUpdate()
        {
            rb.AddForce(Vector3.down * 150);
            if (isPerformingAttack)
            {
                isPerformingAttack = false;
                Vector3 attackDir = PlayerController.Instance.transform.position - transform.position;
                rb.AddForce(attackDir.normalized * agentDashSpeed, ForceMode.Impulse);
            }
        }

        protected override void Update()
        {
            base.Update();
            if(isInDashAttackCoroutine) transform.DOLookAt(PlayerController.Instance.transform.position, 0.2f);
        }


        protected override void DestroyLogic()
        {
            GameManager.Instance.aiPawnsAvailable.Remove(this);
            Pooling.instance.DelayedDePop("Trashmob", gameObject, 0);
        }
        

        
        // Debug -------------------------
        #if UNITY_EDITOR
        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            if(Application.isPlaying)
            {
                Handles.Label(transform.position + Vector3.up, $"walkingSpeed {navMeshAgent.speed}");
                Handles.Label(transform.position + (Vector3.right * 12), $"isPerformingAttack {isPerformingAttack}");
            }
            
            var tr = transform;
            var pos = tr.position;
            // display an orange disc where the object is
            var color = new Color32(0, 125, 255, 20);
            Handles.color = color;
            Handles.DrawSolidDisc(pos, tr.up, agentSpeedRadius);
            
            var color2 = new Color32(255, 125, 255, 40);
            Handles.color = color2;
            Handles.DrawSolidDisc(pos, tr.up, agentDashRadius);

            CacAttackGizmo();
        }

        private void CacAttackGizmo()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, transform.forward * hit.distance);
            Gizmos.DrawWireCube(transform.position + transform.forward * hit.distance, transform.localScale*0.5f);
            
            if (hasHit)
            {
                
            }
        }
        #endif  
    }
}
