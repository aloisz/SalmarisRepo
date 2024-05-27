using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using CameraBehavior;
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
        [SerializeField] protected float agentSpeedWhenIsToFar = 50;
        
        
        protected enum TrashMobState
        {
            Idle,
            Moving,
            AttackingCloseRange
        }

        protected virtual void Start()
        {
            base.Start();
        }
        
        protected override void Update()
        {
            base.Update();
            if(trashMobState == TrashMobState.AttackingCloseRange) 
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
        
        protected override void DestroyLogic()
        {
            Pooling.instance.DePop(so_IA.poolingName, gameObject);
        }
        

        
        // Debug -------------------------
        #if UNITY_EDITOR
        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            if(Application.isPlaying)
            {
                //Handles.Label(transform.position + Vector3.up, $"walkingSpeed {navMeshAgent.speed}");
            }
        }
        #endif  
    }
}
