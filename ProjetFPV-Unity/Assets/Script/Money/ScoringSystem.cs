using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class ScoringSystem : GenericSingletonClass<ScoringSystem>
{
    [SerializeField] int arbitraryValueDebug;

    [SerializeField] List<TimeInterval> timeIntervals = new List<TimeInterval>();
    [SerializeField] List<DeathPenalties> deathPenalties = new List<DeathPenalties>();
    
    private int _timeScore;
    private int _deathBonus;
    
    public override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        GameManager.Instance.OnLevelCompleted += ScoreEndLevel;
        Director.Instance.onArenaFinished += ScoreEndArena;
    }

    private void ScoreEndLevel()
    {
        GameManager.Instance.globalScore = CalculateEndLevelScore();
        PlayerMoney.Instance.Money += GameManager.Instance.globalScore;
        
        Debug.Log(CalculateEndLevelScore());
    }

    private void ScoreEndArena()
    {
        PlayerMoney.Instance.Money += CalculateEndArenaScore();
    }

    private int CalculateEndLevelScore()
    {
        var totalInt = Director.Instance.totalIntensityValueLevel;
        var tScore = _timeScore;
        var deaths = GetDeathScore();

        return totalInt + tScore + deaths + arbitraryValueDebug;
    }
    
    private int CalculateEndArenaScore()
    {
        var totalInt = Director.Instance.totalIntensityValue;
        var deaths = GetDeathScore();
        var overPerf = Director.Instance.playerOverPerformAmount;

        var value = totalInt + deaths + arbitraryValueDebug;
        
        return (int)(totalInt + deaths + ((value * 0.05f) * overPerf) + arbitraryValueDebug);
    }

    private void Update()
    {
        CheckScoreFromTime();
    }

    private void CheckScoreFromTime()
    {
        foreach (TimeInterval ti in timeIntervals)
        {
            if (Director.Instance.levelTimer < ti.timeInSeconds)
            {
                _timeScore = ti.bonus;
                break;
            }
        }
    }

    private int GetDeathScore()
    {
        _deathBonus = Director.Instance.numberOfDeath;
        return _deathBonus;
    }
}

[Serializable]
public class TimeInterval
{
    public float timeInSeconds;
    [InfoBox("Put negative value for a malus. A positive one of a bonus.")]
    public int bonus;
}

[Serializable]
public class DeathPenalties
{
    public int amountOfDeath;
    [InfoBox("Put negative value for a malus. A positive one of a bonus.")]
    public int bonus;
}
