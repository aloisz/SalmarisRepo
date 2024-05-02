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

    public int currentLevelIndex;

    public Action OnLevelCompleted;

    public void LevelFinished() => OnLevelCompleted?.Invoke();
}
