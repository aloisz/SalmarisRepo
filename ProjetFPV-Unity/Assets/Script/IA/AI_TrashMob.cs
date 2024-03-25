using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEditor;
using UnityEngine;


namespace AI
{
    public class AI_TrashMob : AI_Pawn
    {
        [Header("--- AI_TrashMob ---")]
        [SerializeField] protected float agentSpeedRadius = 2;
        [SerializeField] protected float agentSpeedWhenIsToFar = 50;
        [Space]
        [SerializeField] protected float agentDashRadius = 2;
        [SerializeField] protected float agentDashSpeed = 500;
        [SerializeField][Tooltip("How many time the check is performed")] protected float tickVerification = 0.2f;
        protected override void Start()
        {
            base.Start();
            StartCoroutine(CheckDistance());
        }
        
        /// <summary>
        /// Adapt the agent speed if the player is too far away
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator CheckDistance()
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
                    yield return StartCoroutine(Attack());
                }
            }
            
            yield return new WaitForSeconds(tickVerification);
            StartCoroutine(CheckDistance());
        }

        private bool isPerformingAttack;
        private IEnumerator Attack()
        {
            navMeshAgent.speed = 0;
            yield return new WaitForSeconds(1f);
            navMeshAgent.enabled = false;
            rb.isKinematic = false;
            isPerformingAttack = true;
            yield return new WaitForSeconds(2);
            navMeshAgent.enabled = true;
            rb.isKinematic = true;
        }

        private void FixedUpdate()
        {
            if (isPerformingAttack)
            {
                isPerformingAttack = false;
                Vector3 attackDir = PlayerController.Instance.transform.position - transform.position;
                rb.AddForce(attackDir * agentDashSpeed);
            }
        }


        protected override void DestroyLogic()
        {
            Pooling.instance.DelayedDePop("Trashmob", gameObject, 0);
        }

        
        // Debug -------------------------
        #if UNITY_EDITOR
        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            Handles.Label(transform.position + Vector3.up, $"walkingSpeed {navMeshAgent.speed}");
            
            
            var tr = transform;
            var pos = tr.position;
            // display an orange disc where the object is
            var color = new Color32(0, 125, 255, 20);
            Handles.color = color;
            Handles.DrawSolidDisc(pos, tr.up, agentSpeedRadius);
            
            var color2 = new Color32(255, 125, 255, 40);
            Handles.color = color2;
            Handles.DrawSolidDisc(pos, tr.up, agentDashRadius);
        }
        #endif
    }
}
