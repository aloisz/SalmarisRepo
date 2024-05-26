using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    public class AI_SmasherAnimator : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        
        //component
        private AI_Pawn aiPawn;
        
        private string currentState;
        internal string IDLE = "Idle";
        internal string WALK = "Walk";
        internal string ATTACK = "Attack";
        internal string ESTOC = "Estoc";
        internal string ROTATE = "Rotate";
        internal string DEATH = "Death";

        private void Start()
        {
            aiPawn = GetComponent<AI_Pawn>();
        }

        private void Update()
        {
            if (isAnimationPlaying(animator, ESTOC)) return;
            if (isAnimationPlaying(animator, ATTACK)) return;
            
            ChangeState(aiPawn.navMeshAgent.speed > 1 ? WALK : IDLE);
        }


        public void ChangeState(string newState)
        {
            if(newState == currentState) return;
            animator.Play(newState);
            currentState = newState;
        }

        public bool isAnimationPlaying(Animator animator, string stateName)
        {
            return animator.GetCurrentAnimatorStateInfo(0).IsName(stateName) &&
                   animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f;
        }
    }
}

