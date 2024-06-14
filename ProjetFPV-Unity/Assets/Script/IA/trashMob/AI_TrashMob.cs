using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using CameraBehavior;
using DG.Tweening;
using MyAudio;
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
        [SerializeField] protected float agentSpeedWhenIsToFar = 50;
        
        
        protected enum TrashMobState
        {
            Idle,
            Moving,
            AttackingCloseRange
        }

        protected virtual void Awake()
        {
            base.Awake();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            //agentLinkMover.StopCoroutine(agentLinkMover.StartLinkerVerif());
        }
        
        

        public override void ResetAgent()
        {
            base.ResetAgent();
            if(!navMeshAgent.isOnNavMesh) return; 
            navMeshAgent.ResetPath();
            navMeshAgent.CompleteOffMeshLink();
            
            //BUG
            //AudioManager.Instance.SpawnAudio3D(transform.position, SfxType.SFX, 9, 1, 0, 1);

            if(gameObject.activeSelf) agentLinkMover.StartCoroutine(agentLinkMover.StartLinkerVerif());
            
            AudioManager.Instance.SpawnAudio3D(transform.position, SfxType.SFX, 8, 1, 0, 1,1, 0,
                AudioRolloffMode.Logarithmic, 5,20);
        }
        
        protected override void Update()
        {
            base.Update();
            if(trashMobState == TrashMobState.AttackingCloseRange && !isPawnDead) 
                transform.DOLookAt(PlayerController.Instance.transform.position + Vector3.up, 0.2f, AxisConstraint.Y);
        }

        protected TrashMobState ChangeState(TrashMobState state)
        {
            return this.trashMobState = state;
        }
        
        protected override void PawnBehavior()
        {
            base.PawnBehavior();
            CheckDistance();
        }
        
        /// <summary>
        /// Adapt the agent speed if the player is too far away
        /// </summary>
        /// <returns></returns>
        protected virtual void CheckDistance()
        {
            switch (Vector3.Distance(PlayerController.Instance.transform.position, transform.position))
            {
                case var value when value < perimeters[perimeters.Count - 2].distToEnemy:
                    navMeshAgent.speed = so_IA.walkingSpeed;
                    break;
                
                case var value when value < perimeters[perimeters.Count - 1].distToEnemy:
                    navMeshAgent.speed = agentSpeedWhenIsToFar;
                    ChangeState(TrashMobState.Moving);
                    break;
            }
        }

        private bool doHitSd;
        public override void Hit(float damageInflicted)
        {
            base.Hit(damageInflicted);
            
            if(doHitSd) return;
            doHitSd = true;
            StartCoroutine(SoundHitCoroutine());
        }

        IEnumerator SoundHitCoroutine()
        {
            AudioManager.Instance.SpawnAudio3D(transform.position, SfxType.SFX, 10, 1, 0, 1);
            yield return new WaitForSeconds(.1f);
            doHitSd = false;
        }
        
        public override void DestroyLogic()
        {
            AudioManager.Instance.SpawnAudio3D(transform.position, SfxType.SFX, 11, 1, 0, 1);
            Pooling.instance.DelayedDePop(so_IA.poolingName, gameObject, deathDelay);
        }
        
        // Debug -------------------------
        #if UNITY_EDITOR
        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            if(Application.isPlaying)
            {
                //Handles.Label(transform.position + Vector3.up, navMeshAgent.enabled.ToString(), new GUIStyle(){fontSize = 80});
            }
        }
        #endif  
    }
}
