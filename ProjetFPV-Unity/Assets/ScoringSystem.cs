using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class ScoringSystem : GenericSingletonClass<ScoringSystem>
{
    public int globalIntensityValue = 0;
    public int timeScore;

    public List<TimeInterval> timeIntervals = new List<TimeInterval>();
    public List<DeathPenalties> deathPenalties = new List<DeathPenalties>();

    private void Start()
    {
        GameManager.Instance.OnLevelCompleted += ScoreEndLevel;
    }

    public void ScoreEndLevel()
    {
        
    }

    public void ScoreEndArena()
    {
        
    }
}

[Serializable]
public class TimeInterval
{
    public float timeInSeconds;
    [InfoBox("Put negative value for a malus. A positive one of a bonus.")]
    public float bonus;
}

[Serializable]
public class DeathPenalties
{
    public int amountOfDeath;
    [InfoBox("Put negative value for a malus. A positive one of a bonus.")]
    public float bonus;
}
