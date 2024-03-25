using System.Collections;
using System.Collections.Generic;using AI;
using UnityEngine;

public class AI_AirSack : AI_Pawn 
{
    protected override void DestroyLogic()
    {
        //TODO : Implement Pooling
        Destroy(gameObject);
    }
}
