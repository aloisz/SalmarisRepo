using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using Player;
using Weapon;

namespace AI
{
    public class AI_TrashMob_Shooter : AI_TrashMob
    {
        [Header("--- Shooter ---")] 
        [SerializeField] protected float agentShootingRadius = 2;
        
        protected WeaponManager weapon;
        protected bool isShooting;
        
        protected override void Start()
        {
            base.Start();
            weapon = GetComponent<WeaponManager>();
        }

        protected override void Update()
        {
            base.Update();
            ChangeStateMachine();
        }
        
        protected override void PawnBehavior()
        {
            base.PawnBehavior();
            switch (trashMobState)
            {
                case TrashMobState.Idle:
                    isShooting = true;
                    weapon.ShootingAction();
                    break;
                case TrashMobState.Moving:
                    isShooting = false;
                    break;
                case TrashMobState.AttackingCloseRange:
                    isShooting = false;
                    break;
            }
        }

        private float time = 0;
        private float maxTimer = 5;
        
        private void ChangeStateMachine()
        {
            if (time < maxTimer)
            {
                time += Time.deltaTime * 1;
                if (time >= maxTimer)
                {
                    time = 0;
                    ChangeState(TrashMobState.Idle);
                }
            }
        }

        protected override void CheckDistance()
        {
            if (Vector3.Distance(PlayerController.Instance.transform.position, transform.position) > agentShootingRadius)
            {
                base.CheckDistance();
            }
            else
            {
                navMeshAgent.speed = so_IA.walkingSpeed;
            }
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