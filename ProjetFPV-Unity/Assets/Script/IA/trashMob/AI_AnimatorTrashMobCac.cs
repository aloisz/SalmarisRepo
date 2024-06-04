using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEngine;

namespace AI
{
    public class AI_AnimatorTrashMobCac : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        //component
        private AI_Pawn aiPawn;
        
        private string currentState;
        internal string SPAWN = "Spawn";
        internal string IDLE = "Idle";
        internal string WALK = "Walk";
        internal string ATTACK = "Attack";
        internal string JUMP = "Jump";
        internal string DEATH = "Death";
        
        private void Start()
        {
            aiPawn = GetComponent<AI_Pawn>();
        }

        private void Update()
        {
            
            if (isAnimationPlaying(animator, SPAWN)) return;
            if (isAnimationPlaying(animator, DEATH)) return;
            if (isAnimationPlaying(animator, JUMP)) return;
            if (isAnimationPlaying(animator, ATTACK)) return;
            
            if(aiPawn.isPawnDead) return;
            ChangeState(aiPawn.navMeshAgent.speed > 1 ? WALK : IDLE, .2f);
        }

        public void ChangeState(string newState, float crossFadeDuration)
        {
            if (animator.GetNextAnimatorStateInfo(0).IsName(newState) ||
                animator.GetCurrentAnimatorStateInfo(0).IsName(newState)) return;
            currentState = newState;
            animator.CrossFade(currentState, crossFadeDuration);
        }

        private bool isAnimationPlaying(Animator animator, string stateName)
        {
            return animator.GetCurrentAnimatorStateInfo(0).IsName(stateName) &&
                   animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f;
        }
    }
}
