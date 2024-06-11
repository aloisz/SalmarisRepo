using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using DG.Tweening;
using Player;
using Weapon;

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
        
        protected override void Awake()
        {
            base.Awake();
            weapon = GetComponent<WeaponManager>();
            animatorTrashShooter = GetComponent<Ai_AnimatorTrashShooter>();
            if (isPawnStatic) agentLinkMover.enabled = false;
        }

        protected void ResetAgent()
        {
            base.ResetAgent();
            time = 0;
        }

        protected override void Update()
        {
            base.Update();
            ChangeStateWhenReloading();
            if(trashMobState == TrashMobState.Idle && !isPawnDead) 
                transform.DOLookAt(PlayerController.Instance.transform.position + Vector3.up, 0.2f, AxisConstraint.Y);
            
            switch (Vector3.Distance(PlayerController.Instance.transform.position, transform.position))
            {
                case var value when value < agentShootingRadius:
                    HandlShooting();
                    break;
                case var value when value > agentShootingRadius:
                    if(isPawnStatic)return;
                    ChangeState(TrashMobState.Moving);
                    break;
            }
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

        private float time = 0;
        [SerializeField] float timeBeforeShooting = 5;
        
        private void HandlShooting()
        {
            if (time < timeBeforeShooting)
            {
                time += Time.deltaTime * 1;
                if (time >= timeBeforeShooting)
                {
                    time = 0;
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

        protected override void CheckDistance()
        {
            base.CheckDistance();
        }
        
        public override void DestroyLogic()
        {
            base.DestroyLogic();
            animatorTrashShooter.ChangeState(animatorTrashShooter.DEATH, .5f);
            if(gameObject.activeSelf) agentLinkMover.StopCoroutine(agentLinkMover.StartLinkerVerif());
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