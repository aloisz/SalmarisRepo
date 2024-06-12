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
        private AI_TrashMob_Cac aiPawnCac;
        
        private string currentState;
        internal string SPAWN = "Spawn";
        internal string IDLE = "Idle";
        internal string WALK = "Walk";
        internal string ATTACK = "Attack";
        internal string PREATTACK = "PreAttack";
        internal string PREATTACKLOOP = "PreAttackLoop";
        internal string JUMP = "Jump";
        internal string DEATH = "Death";
        
        private void Start()
        {
            aiPawn = GetComponent<AI_Pawn>();
            aiPawnCac = GetComponent<AI_TrashMob_Cac>();
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
