using System;
using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class GameManager : GenericSingletonClass<GameManager>
{
    public List<AI_Pawn> aiPawnsAvailable;
    public int currentLevelIndex;
    
    public Action OnLevelCompleted;

    public int globalScore;

    private void Start()
    {
        
    }

    public void LevelFinished()
    {
        OnLevelCompleted?.Invoke();
        Debug.Log("Level Finished");

        currentLevelIndex++;
    }
}
