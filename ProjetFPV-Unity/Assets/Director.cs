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

    //---------------------------------------
    
    public void EnteringNewArena(int arenaID)
    {
        currentArenaIndex = arenaID;
        
        StartCoroutine(nameof(StartNewWave));
    }

    IEnumerator StartNewWave()
    {
        if(currentWaveIndex < GetActualArenaData().arenaWaves.Length) currentWaveIndex++;
        dynamicNextWaveValue = GetActualWave().nextWaveValue;
        
        if (GetActualWave().enemiesToSpawn.Length <= 0)
            throw new Exception($"{GetActualWave()} doesn't have any enemy to spawn.");
        
        playerPerformance = 0f;
        currentWaveIntensity = 0;
        dynamicNextWaveValue = 0f;

        yield return new WaitForSeconds(GetActualWave().delayBeforeSpawn);

        StartCoroutine(nameof(SpawnEnemies));
        StartCoroutine(nameof(CalculateWaveIntensity));
        StartCoroutine(nameof(CalculatePlayerPerformance));
    }

    private IEnumerator SpawnEnemies()
    {
        int i = 0;
        foreach (EnemyToSpawn e in GetActualWave().enemiesToSpawn)
        {
            GameObject mob = Pooling.instance.Pop(Enum.GetName(typeof(EnemyToSpawn.EnemyKeys), e.enemyKey));
            mob.transform.position = e.worldPosition;
            _spawnedEnemies.Add(mob.GetComponent<AI_Pawn>());

            yield return new WaitForSeconds(GetActualWave().delayBetweenEachEnemySpawn);

            i++;
        }
    }

    private void EndWave()
    {
        /*_spawnedEnemies.Clear();
        playerPerformance = 0f;

        if (GetActualArenaData().arenaWaves.Length == currentWaveIndex + 1) return;

        currentWaveIndex++;
        
        StartCoroutine(nameof(StartNewWave));*/
    }

    private IEnumerator CalculatePlayerPerformance()
    {
        Debug.Log("Player Perf Calculation");
        
        //Loop on every spawned enemies to get their health.
        foreach (AI_Pawn ai in _spawnedEnemies) 
            if (ai.actualPawnHealth <= 0) AddPlayerPerformance(ai.enemyWeight);
        
        //Compare the actual player performance and the current wave's reference performance.
        if(playerPerformance > GetActualWave().performanceReference) 
            AddNextWaveValue(GetActualWave().nextWaveValueAddedValue);
        
        yield return new WaitForSeconds(playerPerformanceComparisonDelay);
        
        StartCoroutine(nameof(CalculatePlayerPerformance));
    }

    private void AddPlayerPerformance(float amount) => playerPerformance += amount;

    private IEnumerator CalculateWaveIntensity()
    {
        Debug.Log("Wave Intensity Calculation");
        if (currentWaveIntensity <= dynamicNextWaveValue)
        {
            //EndWave();
        }
        
        foreach (AI_Pawn ai in _spawnedEnemies)
        {
            currentWaveIntensity += ai.enemyWeight;
        }
        
        yield return new WaitForSeconds(0.05f);
        StartCoroutine(nameof(CalculateWaveIntensity));
    }

    private void AddNextWaveValue(float amount) => dynamicNextWaveValue += amount;

    private void CheckEnemyCount()
    {
        var i = 0;
        foreach (AI_Pawn e in _spawnedEnemies)
        {
            if (e.actualPawnHealth > 0) i++;
        }
        _currentRemainingEnemies = i;
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
        CheckEnemyCount();
    }
}
