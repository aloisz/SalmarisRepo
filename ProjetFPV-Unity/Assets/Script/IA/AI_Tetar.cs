using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AI
{
    public class AI_Tetar : AI_Pawn
    {

        protected override void Start()
        {
            base.Start();
            navMeshAgent.speed = walkingSpeed;
            navMeshAgent.stoppingDistance = stoppingDistance;
        }
    }
}
