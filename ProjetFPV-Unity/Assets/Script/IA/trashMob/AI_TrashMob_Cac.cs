using System.Collections;
using System.Collections.Generic;
using CameraBehavior;
using MyAudio;
using NaughtyAttributes;
using Player;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace AI
{
    public class AI_TrashMob_Cac: AI_TrashMob
    { 
        [SerializeField] protected float agentDashSpeed = 500;
        private int actualCountBeforeAttack = 0;
        
        [Header("--- TransformPos ---")] 
        [SerializeField] protected Transform cacAttackPos;
        
        [Header("Properties")] 
        [SerializeField] [Range(0,20)] protected float cacAttackSphereRadius;
        [ReadOnly] [SerializeField] protected bool isCacAttacking = false;
        [SerializeField] protected float damageApplied;
        
        [Header("Camera Shake when hit")] 
        [SerializeField] private float shakeDuration = .1f;
        [SerializeField] private float shakeMagnitude = 20f;
        [SerializeField] private float shakeFrequency = .5f;
        [SerializeField] private float power = 4;
        
        // Component
        protected internal AI_AnimatorTrashMobCac animatorTrashMobCac;

        protected override void Awake()
        {
            base.Awake();
            animatorTrashMobCac = GetComponent<AI_AnimatorTrashMobCac>();
        }

        public override void ResetAgent()
        {
            base.ResetAgent();
        }
        
        protected override void PawnBehavior()
        {
            base.PawnBehavior();
            CheckDistance();
        }
        
        protected virtual void CheckDistance()
        {
            
            switch (Vector3.Distance(PlayerController.Instance.transform.position, transform.position))
            {
                case var value when value < perimeters[0].distToEnemy:
                    actualCountBeforeAttack++;
                    if (actualCountBeforeAttack >= perimeters[0].timeSpentInPerimeter)
                    {
                        ChangeState(TrashMobState.AttackingCloseRange);
                        actualCountBeforeAttack = 0;
                        isCacAttacking = true;
                    }
                    break;
                
                case var value when value < perimeters[1].distToEnemy:
                    if(isInDashAttackCoroutine) return;
                    actualCountBeforeAttack++;
                    
                    if (actualCountBeforeAttack >= perimeters[1].timeSpentInPerimeter && !isInDashAttackCoroutine)
                    {
                        actualCountBeforeAttack = 0;
                        ChangeState(TrashMobState.AttackingCloseRange);
                        StartCoroutine(DashAttack());
                    }
                    break;
                
                case var value when value < perimeters[2].distToEnemy:
                    navMeshAgent.speed = so_IA.walkingSpeed;
                    isCacAttacking = false;
                    break;
                
                case var value when value < perimeters[3].distToEnemy:
                    navMeshAgent.speed = agentSpeedWhenIsToFar;
                    ChangeState(TrashMobState.Moving);
                    isCacAttacking = false;
                    break;
            }
            base.CheckDistance();
        }

        protected override void Update()
        {
            base.Update();
            CacAttackPerimeter0();
        }

        private bool isPerformingDashPhysics = false;
        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isPerformingDashPhysics && !isPawnDead)
            {
                isPerformingDashPhysics = false;
                Vector3 attackDir = PlayerController.Instance.transform.position - transform.position;
                rb.AddForce(attackDir.normalized * agentDashSpeed, ForceMode.Impulse);
            }
        }
        
        public override void DisableAgent()
        {
            base.DisableAgent();
            StopCoroutine(DashAttack());
        }
        
        private bool isInDashAttackCoroutine;
        private IEnumerator DashAttack()
        {
            isInDashAttackCoroutine = true;
            navMeshAgent.speed = 0;
            animatorTrashMobCac.ChangeState(animatorTrashMobCac.JUMP, 2f);

            yield return new WaitForSeconds(1f);
            IsPhysicNavMesh(false);
            isCacAttacking = true;
            isPerformingDashPhysics = true;

            yield return new WaitForSeconds(2);
            IsPhysicNavMesh(true);
            ChangeState(TrashMobState.Moving);
            actualCountBeforeAttack = 0;
            isInDashAttackCoroutine = false;
            isCacAttacking = false;
        }
        
        
        public override void DestroyLogic()
        {
            if(gameObject.activeSelf) agentLinkMover.StopCoroutine(agentLinkMover.StartLinkerVerif());
            
            base.DestroyLogic();
            
            animatorTrashMobCac.ChangeState(animatorTrashMobCac.DEATH,.2f);

            StopCoroutine(DashAttack());
            IsPhysicNavMesh(false);
            actualCountBeforeAttack = 0;
            isInDashAttackCoroutine = false;
            isCacAttacking = false;
            
            StartCoroutine(nameof(DeathKnockBack));
        }

        IEnumerator DeathKnockBack()
        {
            yield return new WaitUntil(() => !navMeshAgent.enabled);
            rb.AddForce(PlayerController.Instance.transform.forward * knockBackDeathIntensity, ForceMode.Impulse);
        }
        
        
        #region Attack

        private IDamage pl;
        private void CacAttackPerimeter0()
        {
            if(!isCacAttacking) return;
            if(isPawnDead) return;
            
            Collider[] colliders = Physics.OverlapSphere(cacAttackPos.position, cacAttackSphereRadius, targetMask);
            animatorTrashMobCac.ChangeState(animatorTrashMobCac.ATTACK,.2f);
            foreach (var obj in colliders)
            {
                if (obj.transform.CompareTag("Player"))
                {
                    if (obj.transform.TryGetComponent(out pl))
                    {
                        pl.Hit(damageApplied);
                        isCacAttacking = false;
                    }
                    
                    AudioManager.Instance.SpawnAudio3D(transform.position, SfxType.SFX, 12, 1, 0, 1);
                    CameraShake.Instance.ShakeCamera(false, shakeDuration, shakeMagnitude, shakeFrequency, true, power);
                }
            }
        }
        #endregion
        
#if UNITY_EDITOR
        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
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
}