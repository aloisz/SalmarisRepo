using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyAudio;
using Player;
using Weapon;
using Random = UnityEngine.Random;

namespace AI
{
    public class AI_TrashMob_Shooter : AI_TrashMob
    {
        [Header("--- Shooter ---")] 
        [SerializeField] protected bool isPawnStatic;
        [SerializeField] protected float agentShootingRadius = 2;
        
        protected WeaponManager weapon;
        protected bool isShooting;
        internal Ai_AnimatorTrashShooter animatorTrashShooter;
        protected internal Animator animator;
        protected internal CapsuleCollider collider;
        
        [Header("Ragdoll")] 
        [SerializeField] protected internal float ragdollMass;
        [SerializeField] protected internal List<Rigidbody> ragDollRbs;
        [SerializeField] protected internal List<CharacterJoint> characterJoints;
        [SerializeField] protected internal List<CapsuleCollider> capsuleColliders;
        private bool isKnockback = false;
        private bool addRotation = false;
        
        protected override void Awake()
        {
            base.Awake();
            weapon = GetComponent<WeaponManager>();
            animatorTrashShooter = GetComponent<Ai_AnimatorTrashShooter>();
            animator = GetComponentInChildren<Animator>();
            collider = GetComponent<CapsuleCollider>();
            if (isPawnStatic) agentLinkMover.enabled = false;
            
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

        public override void ResetAgent(bool doAudio)
        {
            base.ResetAgent(doAudio);
            time = 0;
            trashMobState = TrashMobState.Moving;
            countAction = 0;
            if (isPawnStatic) agentLinkMover.enabled = false;
            
            //ragdoll 
            
            foreach (var rb in ragDollRbs)
            {
                rb.isKinematic = true;
                rb.velocity = Vector3.zero;
                rb.rotation = Quaternion.identity;
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
            animatorTrashShooter.enabled = true;
            rb.isKinematic = true;
            animator.enabled = true;
            collider.enabled = true;
        }

        protected override void Update()
        {
            base.Update();
            if(trashMobState == TrashMobState.Idle && !isPawnDead) 
                transform.DOLookAt(PlayerController.Instance.transform.position + Vector3.up, 0.2f, AxisConstraint.Y);
            
            ChangeStateWhenReloading();
            
            switch (Vector3.Distance(PlayerController.Instance.transform.position, transform.position))
            {
                case var value when value < agentShootingRadius:
                    HandlShooting();
                    break;
                case var value when value > agentShootingRadius:
                    if(isPawnStatic)break;
                    ChangeState(TrashMobState.Moving);
                    break;
            }
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            //Ragdoll
            if (isPawnDead)
            {
                foreach (var rb in ragDollRbs)
                {
                    rb.AddForce(Vector3.down * knockBackDeathIntensityXYZ.x);
                    if (addRotation)
                    {
                        addRotation = false;
                        GetRandomRotation();
                        rb.MoveRotation(rb.rotation * Quaternion.Euler(rotValue));
                    }
                }
            }
            if(!isKnockback && isPawnDead) return;
            foreach (var rb in ragDollRbs)
            {
                rb.AddForce((PlayerController.Instance.transform.forward * knockBackDeathIntensityXYZ.y + 
                            (Vector3.up * knockBackDeathIntensityXYZ.z)) * knockBackMultiplier, ForceMode.Impulse);
            }
            isKnockback = false;
        }
        private Vector3 rotValue;
        private void GetRandomRotation()
        {
            float randomValue = Random.Range(12000, 15000);
            rotValue = new Vector3(randomValue, randomValue, randomValue);
        }
        
        protected override void PawnBehavior()
        {
            base.PawnBehavior();
            switch (trashMobState)
            {
                case TrashMobState.Idle:
                    isShooting = true;
                    weapon.ShootingAction();
                    IsPhysicNavMesh(false);
                    countAction++;
                    AudioManager.Instance.SpawnAudio3D(transform.position, SfxType.SFX, 12, .6f, 0, 1, 1,0,
                        AudioRolloffMode.Linear, 1,100);
                    if(countAction <= 2) break;
                    ChangeState(TrashMobState.Moving);
                    break;
                case TrashMobState.Moving:
                    if(isPawnStatic)return;
                    isShooting = false;
                    IsPhysicNavMesh(true);
                    break;
                case TrashMobState.AttackingCloseRange:
                    isShooting = false;
                    break;
            }
        }
        
        protected override void CheckDistance()
        {
            base.CheckDistance();
        }

        private int countAction = 0;
        private float time = 0;
        [SerializeField] float timeBeforeShooting = 5;
        
        private void HandlShooting()
        {
            if (time < timeBeforeShooting && trashMobState == TrashMobState.Moving)
            {
                time += Time.deltaTime * 1; // before was 1
                if (time >= timeBeforeShooting)
                {
                    time = 0;
                    countAction = 0;
                    ChangeState(TrashMobState.Idle);
                }
            }
        }

        private void ChangeStateWhenReloading()
        {
            if (weapon.isReloading)
            {
                ChangeState(TrashMobState.Moving);
            }
        }

        public override void DisableAgent()
        {
            base.DisableAgent();
            animatorTrashShooter.ChangeState(animatorTrashShooter.IDLE, 0f);
        }

        public override void DestroyLogic()
        {
            base.DestroyLogic();
            animatorTrashShooter.ChangeState(animatorTrashShooter.DEATH, .5f);
            if(gameObject.activeSelf) agentLinkMover.StopCoroutine(agentLinkMover.StartLinkerVerif());
            
            ChangeState(TrashMobState.Idle);
            IsPhysicNavMesh(false);
            StartCoroutine(nameof(DeathKnockBack));
            
            //Ragdoll
            animatorTrashShooter.enabled = false;
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
            addRotation = true;
        }

#if UNITY_EDITOR
        

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            var tr = transform;
            var pos = tr.position;
            // display an orange disc where the object is
            var color = new Color32(255, 125, 0, 20);
            Handles.color = color;
            Handles.DrawSolidDisc(pos, tr.up, agentShootingRadius);
        }
#endif
    }
}