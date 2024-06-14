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
        [ReadOnly] [SerializeField] internal bool isCacAttacking = false;
        [SerializeField] protected float damageApplied;
        
        [Header("Camera Shake when hit")] 
        [SerializeField] private float shakeDuration = .1f;
        [SerializeField] private float shakeMagnitude = 20f;
        [SerializeField] private float shakeFrequency = .5f;
        [SerializeField] private float power = 4;
        
        // Component
        protected internal AI_AnimatorTrashMobCac animatorTrashMobCac;
        protected internal Animator animator;
        protected internal CapsuleCollider collider;

        [Header("Ragdoll")] 
        [SerializeField] protected internal float ragdollMass;
        [SerializeField] protected internal List<Rigidbody> ragDollRbs;
        [SerializeField] protected internal List<CharacterJoint> characterJoints;
        [SerializeField] protected internal List<CapsuleCollider> capsuleColliders;
        private bool isKnockback = false;

        protected override void Awake()
        {
            base.Awake();
            animatorTrashMobCac = GetComponent<AI_AnimatorTrashMobCac>();
            animator = GetComponentInChildren<Animator>();
            collider = GetComponent<CapsuleCollider>();

            foreach (Transform t in GetComponentsInChildren<Transform>())
            {
                if (t != transform)
                {
                    if (t.GetComponent<Rigidbody>())
                    {
                        ragDollRbs.Add(t.GetComponent<Rigidbody>());
                    }
                    if (t.GetComponent<CharacterJoint>())
                    {
                        characterJoints.Add(t.GetComponent<CharacterJoint>());
                    }
                    if (t.GetComponent<CapsuleCollider>())
                    {
                        capsuleColliders.Add(t.GetComponent<CapsuleCollider>());
                    }
                }
            }

            foreach (var rb in ragDollRbs)
            {
                rb.isKinematic = true;
                rb.mass = ragdollMass;
            }
            foreach (var capsule in capsuleColliders)
            {
                capsule.enabled = false;
            }
        }

        public override void ResetAgent()
        {
            base.ResetAgent();
            if(animatorTrashMobCac) animatorTrashMobCac.ChangeState(animatorTrashMobCac.SPAWN, 0.1f);
            if(animatorTrashMobCac) animatorTrashMobCac.ChangeState(animatorTrashMobCac.WALK, 1.5f);
            
            
            //ragdoll 
            
            foreach (var rb in ragDollRbs)
            {
                rb.isKinematic = true;
            }
            foreach (var capsule in capsuleColliders)
            {
                capsule.enabled = false;
            }
            isKnockback = false;

            StartCoroutine(ResetAgentCoroutine());
        }

        IEnumerator ResetAgentCoroutine()
        {
            yield return new WaitForSeconds(.1f);
            animatorTrashMobCac.enabled = true;
            rb.isKinematic = false;
            animator.enabled = true;
            collider.enabled = true;
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

            
            //Ragdoll
            if (isPawnDead)
            {
                foreach (var rb in ragDollRbs)
                {
                    rb.AddForce(Vector3.down * knockBackDeathIntensityXYZ.x);
                }
            }
            if(!isKnockback && isPawnDead) return;
            float x = Random.Range(-180, 180);
            float z = Random.Range(-180, 180);
            foreach (var rb in ragDollRbs)
            {
                rb.AddForce(PlayerController.Instance.transform.forward * knockBackDeathIntensityXYZ.y + 
                            (Vector3.up * knockBackDeathIntensityXYZ.z), ForceMode.Impulse);
            }
            isKnockback = false;
        }
        
        public override void DisableAgent()
        {
            base.DisableAgent();
            StopCoroutine(DashAttack());
        }
        
        internal bool isInDashAttackCoroutine;
        private IEnumerator DashAttack()
        {
            isInDashAttackCoroutine = true;
            navMeshAgent.speed = 0;
            IsPhysicNavMesh(false);
            
            animatorTrashMobCac.ChangeState(animatorTrashMobCac.PREATTACK, 0.1f);
            
            yield return new WaitForSeconds(0.35f);
            
            animatorTrashMobCac.ChangeState(animatorTrashMobCac.PREATTACKLOOP, 0.25f);

            yield return new WaitForSeconds(0.9f);
            
            animatorTrashMobCac.ChangeState(animatorTrashMobCac.JUMP, 0.1f);
            
            yield return new WaitForSeconds(0.1f);
            
            isCacAttacking = true;
            isPerformingDashPhysics = true;
            
            yield return new WaitForSeconds(0.75f);
            
            animatorTrashMobCac.ChangeState(animatorTrashMobCac.IDLE, 0.1f);

            yield return new WaitForSeconds(0.25f);
            
            IsPhysicNavMesh(true);
            ChangeState(TrashMobState.Moving);
            animatorTrashMobCac.ChangeState(animatorTrashMobCac.WALK, 0.1f);
            
            actualCountBeforeAttack = 0;
            isInDashAttackCoroutine = false;
            //isCacAttacking = false;
        }
        
        
        public override void DestroyLogic()
        {
            if(gameObject.activeSelf) agentLinkMover.StopCoroutine(agentLinkMover.StartLinkerVerif());
            
            base.DestroyLogic();
            StopAllCoroutines();
            animatorTrashMobCac.ChangeState(animatorTrashMobCac.DEATH,.2f);
            
            IsPhysicNavMesh(false);
            actualCountBeforeAttack = 0;
            isInDashAttackCoroutine = false;
            isCacAttacking = false;
            
            animatorTrashMobCac.enabled = false;
            rb.isKinematic = true;
            navMeshAgent.enabled = false;
            animator.enabled = false;
            collider.enabled = false;
            
            foreach (var rb in ragDollRbs)
            {
                rb.isKinematic = false;
            }
            foreach (var capsule in capsuleColliders)
            {
                capsule.enabled = true;
            }
            
            StartCoroutine(nameof(DeathKnockBack));
        }

        IEnumerator DeathKnockBack()
        {
            yield return new WaitUntil(() => !navMeshAgent.enabled && gameObject.activeSelf);
            isKnockback = true;
        }
        
        
        #region Attack

        private IDamage pl;
        private void CacAttackPerimeter0()
        {
            if(!isCacAttacking) return;
            if(isPawnDead) return;
            
            Debug.Log("Enter");
            
            Collider[] colliders = Physics.OverlapSphere(cacAttackPos.position, cacAttackSphereRadius, targetMask);
            if(!isInDashAttackCoroutine) animatorTrashMobCac.ChangeState(animatorTrashMobCac.ATTACK,.2f);
            
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