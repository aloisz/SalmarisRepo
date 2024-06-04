using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    public class Ai_AnimatorTrashShooter : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        //component
        private AI_Pawn aiPawn;
        
        private string currentState;
        internal string SPAWN = "Spawn";
        internal string IDLE = "Idle";
        internal string WALK = "Walk";
        internal string ATTACK = "Attack";
        internal string DEATH = "Death";
        
        private void Start()
        {
            aiPawn = GetComponent<AI_Pawn>();
        }

        private void Update()
        {
            
            if (isAnimationPlaying(animator, SPAWN)) return;
            if (isAnimationPlaying(animator, DEATH)) return;
            if (isAnimationPlaying(animator, ATTACK)) return;
            
            if(aiPawn.isPawnDead) return;
            ChangeState(WALK, .1f);
        }

        public void ChangeState(string newState, float crossFadeDuration)
        {
            currentState = newState;
            animator.Play(currentState);
        }

        private bool isAnimationPlaying(Animator animator, string stateName)
        {
            return animator.GetCurrentAnimatorStateInfo(0).IsName(stateName) &&
                   animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f;
        }
    }
}

