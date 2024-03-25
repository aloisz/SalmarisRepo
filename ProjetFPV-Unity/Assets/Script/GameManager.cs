using System;
using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class GameManager : GenericSingletonClass<GameManager>
{
    public int numberOfTrashMob = 20;
    public List<AI_Pawn> aiPawnsAvailable;
    private void Start()
    {
        for (int i = 0; i < numberOfTrashMob; i++)
        {
            GameObject trashMob = Pooling.instance.Pop("Trashmob");
            trashMob.transform.position = Random.insideUnitSphere * 350;
        }
    }
}
