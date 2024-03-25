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
        private bool isPerformingAttack;
        
        [Space]
        [SerializeField][Tooltip("How many time the check is performed")] protected float tickVerification = 0.2f;
        protected override void Start()
        {
            base.Start();
            StartCoroutine(CheckDistance());
            GameManager.Instance.aiPawnsAvailable.Add(this);
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
        
        private IEnumerator Attack()
        {
            navMeshAgent.speed = 0;
            yield return new WaitForSeconds(1f);
            transform.LookAt(PlayerController.Instance.transform.position);
            navMeshAgent.enabled = false;
            rb.isKinematic = false;
            isPerformingAttack = true;
            yield return new WaitForSeconds(2);
            GetPawnPersonnalInformation();
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
            navMeshAgent.enabled = true;
            rb.isKinematic = true;
            StartCoroutine(CheckDistance());
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
            if(!Application.isPlaying) return;
            Handles.Label(transform.position + Vector3.up, $"walkingSpeed {navMeshAgent.speed}");
            Handles.Label(transform.position + (Vector3.right * 12), $"isPerformingAttack {isPerformingAttack}");
            
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
