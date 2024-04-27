using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AI;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class Director : GenericSingletonClass<Director>
{
    public float currentWaveIntensity;
    public float playerPerformance;
    public float dynamicNextWaveValue;
    
    public float playerPerformanceComparisonDelay = 0.25f;
    
    public int currentArenaIndex;
    public int currentWaveIndex;
    
    public List<ArenaTrigger> arenas = new List<ArenaTrigger>();

    private List<AI_Pawn> _spawnedEnemies = new List<AI_Pawn>();
    
    private int _currentRemainingEnemies;
    private float _timerToCheckPlayerPerformance;
    private float _lastKilledEnemiesValue;

    private bool _isInAArena;
    private bool _isInAWave;
    private bool _hasFinishSpawningEnemies;

    //---------------------------------------
    
    public void EnteringNewArena(int arenaID)
    {
        currentArenaIndex = arenaID;
        
        StartCoroutine(nameof(StartNewWave));
    }

    IEnumerator StartNewWave()
    {
        ResetVariables();

        if (currentWaveIndex < GetActualArenaData().arenaWaves.Length - 1) currentWaveIndex++;
        else yield break;
        
        dynamicNextWaveValue = GetActualWave().nextWaveValue;
        
        if (GetActualWave().enemiesToSpawn.Length <= 0)
            throw new Exception($"{GetActualWave()} doesn't have any enemy to spawn.");

        yield return new WaitForSeconds(GetActualWave().delayBeforeSpawn);

        StartCoroutine(nameof(SpawnEnemies));
    }

    private IEnumerator SpawnEnemies()
    {
        int i = 0;
        _hasFinishSpawningEnemies = false;
        foreach (EnemyToSpawn e in GetActualWave().enemiesToSpawn)
        {
            GameObject mob = Pooling.instance.Pop(Enum.GetName(typeof(EnemyToSpawn.EnemyKeys), e.enemyKey));
            mob.transform.position = e.worldPosition;
            _spawnedEnemies.Add(mob.GetComponent<AI_Pawn>());

            yield return new WaitForSeconds(GetActualWave().delayBetweenEachEnemySpawn);

            i++;
        }
        _hasFinishSpawningEnemies = true;
    }

    private void CalculateWaveIntensityAndRemainingEnemies()
    {
        currentWaveIntensity = 0;
        _currentRemainingEnemies = 0;
        
        foreach (AI_Pawn ai in _spawnedEnemies)
        {
            if (ai.actualPawnHealth > 0)
            {
                currentWaveIntensity += ai.enemyWeight;
                _currentRemainingEnemies += 1;
            }
        }
    }

    private void CompareIntensityAndNextWaveValue()
    {
        if (currentWaveIntensity <= dynamicNextWaveValue && _hasFinishSpawningEnemies)
        {
            StartCoroutine(nameof(StartNewWave));
        }
    }

    private void ComparePlayerPerfAndReferencePerf()
    {
        if (playerPerformance > GetActualWave().performanceReference)
        {
            dynamicNextWaveValue += GetActualWave().nextWaveValueAddedValue;
        }
    }

    private void CalculatePlayerPerformance()
    {
        playerPerformance = _lastKilledEnemiesValue;
    }

    private void TimerCalculationPerformancePlayer()
    {
        _timerToCheckPlayerPerformance.DecreaseTimerIfPositive();
        
        if (_timerToCheckPlayerPerformance <= 0)
        {
            _timerToCheckPlayerPerformance = playerPerformanceComparisonDelay;
            
            ComparePlayerPerfAndReferencePerf();
            _lastKilledEnemiesValue = 0;
        }
    }

    public void TryAddingValueFromLastKilledEnemy(float value)
    {
        _lastKilledEnemiesValue += value;
    }

    private ArenaData GetActualArenaData()
    {
        if (currentArenaIndex < 0) return null;
        return arenas[currentArenaIndex].arenaData;
    }

    private ArenaWave GetActualWave()
    {
        if (currentWaveIndex < 0) return null;
        return GetActualArenaData().arenaWaves[currentWaveIndex]; 
    }

    private void Update()
    {
        _isInAArena = currentArenaIndex >= 0;
        _isInAWave = currentWaveIndex >= 0;

        if (!_isInAArena || !_isInAWave)
        {
            if (_spawnedEnemies.Count > 0) _spawnedEnemies.Clear();
            return;
        }

        CalculateWaveIntensityAndRemainingEnemies();

        CompareIntensityAndNextWaveValue();
        
        TimerCalculationPerformancePlayer();
        CalculatePlayerPerformance();
    }

    private void ResetVariables()
    {
        playerPerformance = 0f;
        currentWaveIntensity = 0;
        dynamicNextWaveValue = 0f;
        _lastKilledEnemiesValue = 0f;
        
        _currentRemainingEnemies = 0;
    }
}
