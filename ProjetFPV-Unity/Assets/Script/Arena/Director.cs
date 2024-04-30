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
    [SerializeField] private bool DEBUG;
    
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

    private bool _currentArenaFinished = true;

    private bool _isInAArena;
    private bool _isInAWave;
    private bool _hasFinishSpawningEnemies;
    private bool _hasStartedWave;

    //---------------------------------------
    
    /// <summary>
    /// Function to start notify the Director that the player is inside an Arena.
    /// </summary>
    /// <param name="arenaID">The Arena ID to start wave of.</param>
    public void EnteringNewArena(int arenaID)
    {
        if (!_currentArenaFinished || arenaID <= currentArenaIndex) return;

        _currentRemainingEnemies = 0;

        //when entering in the arena, notify the director if the new arena ID.
        currentArenaIndex = arenaID;
        
        //start a new wave.
        StartCoroutine(nameof(StartNewWave));
    }

    /// <summary>
    /// Routine to start a new wave.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    IEnumerator StartNewWave()
    {
        //reset all variables.
        ResetVariables();
        
        //check if a wave is remaining to start, if so, increment the current wave index for go to the next one.
        if (CanGoToNextWave()) currentWaveIndex++;
        //else, notify the director that the arena is clear for let the player start another arena (in a different trigger).
        else yield break;

        _currentArenaFinished = false;
        _hasStartedWave = true;

        //set the (dynamic)NextWaveValue to the NextWaveValue reference in the wave data.
        dynamicNextWaveValue = GetActualWave().nextWaveValue;
        
        //if the wave doesn't contain any mob to spawn...
        if (GetActualWave().enemiesToSpawn.Length <= 0)
            throw new Exception($"{GetActualWave()} doesn't have any enemy to spawn.");

        yield return new WaitForSeconds(GetActualWave().delayBeforeSpawn);

        StartCoroutine(nameof(SpawnEnemies));
    }

    /// <summary>
    /// Routine that handles the mob spawning.
    /// </summary>
    /// <returns></returns>
    private IEnumerator SpawnEnemies()
    {
        int i = 0;
        _hasFinishSpawningEnemies = false;
        
        foreach (EnemyToSpawn e in GetActualWave().enemiesToSpawn)
        {
            GameObject mob = Pooling.instance.Pop(Enum.GetName(typeof(EnemyToSpawn.EnemyKeys), e.enemyKey));
            AI_Pawn p = mob.GetComponent<AI_Pawn>();
            p.EnableNavMesh(false);
            
            mob.transform.position = e.worldPosition;
            
            _spawnedEnemies.Add(p);

            yield return new WaitForSeconds(GetActualWave().delayBetweenEachEnemySpawn);
            
            p.EnableNavMesh(true);

            i++;
        }
        
        _hasFinishSpawningEnemies = true;
        _hasStartedWave = false;
    }

    /// <summary>
    /// Calculate the wave intensity based on the current remaining enemies. Calculate it too.
    /// </summary>
    private void CalculateWaveIntensityAndRemainingEnemies()
    {
        //reset both of variables to keep them at the good number (because of the +=)
        currentWaveIntensity = 0;
        _currentRemainingEnemies = 0;
        
        foreach (AI_Pawn ai in _spawnedEnemies)
        {
            if (ai.actualPawnHealth > 0)
            {
                //add every mob's weight to the starting wave intensity.
                currentWaveIntensity += ai.enemyWeight;
                _currentRemainingEnemies += 1;
            }
        }

        if (_currentRemainingEnemies <= 0 && !CanGoToNextWave() && !_currentArenaFinished)
        {
            NotifyArenaCompleted();
        }
    }

    /// <summary>
    /// Compare the intensity of the wave, and the dynamicNextWaveValue for notify if the player is performing too much.
    /// If the dynamicNextWaveValue go too high too fast, the currentWaveIntensity will be inferior, so start the new wave.
    /// </summary>
    private void CompareIntensityAndNextWaveValue()
    {
        if (currentWaveIntensity < dynamicNextWaveValue && _hasFinishSpawningEnemies && !_hasStartedWave)
        {
            StartCoroutine(nameof(StartNewWave));
        }
    }

    /// <summary>
    /// Compare the player's performance with the actual wave's performance reference.
    /// If the player's performance is higher at the comparison moment, then increment the dynamicNextWaveValue from the
    /// current wave's incrementation variable.
    /// </summary>
    private void ComparePlayerPerfAndReferencePerf()
    {
        if (playerPerformance > GetActualWave().performanceReference)
        {
            dynamicNextWaveValue += GetActualWave().nextWaveValueAddedValue;
        }
    }

    /// <summary>
    /// Calculate the player's performance, based on the last killed enemies, in timerToCheckPlayerPerformance seconds.
    /// </summary>
    private void CalculatePlayerPerformance()
    {
        playerPerformance = _lastKilledEnemiesValue;
    }

    /// <summary>
    /// Timer that handle the verification of the player's performance, if the timer runs out, start comparing the player's perf with
    /// the current wave reference one. Reset the timer and the last killed enemies.
    /// </summary>
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

    /// <summary>
    /// Function called when an enemy is dead. It will try to add the weight to the last killed enemy.
    /// </summary>
    /// <param name="value">Enemy's weight.</param>
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

    private void NotifyArenaCompleted()
    {
        _currentArenaFinished = true;
        _hasStartedWave = false;
        currentWaveIndex = -1;
        
        if(GetActualArenaData().shouldSpawnShopAtTheEnd) 
            UpgradeModule.Instance.InitModule(GetActualArenaData().shopOrbitalPosition);
    }

    private void Update()
    {
        _isInAArena = !_currentArenaFinished && _isInAWave;
        _isInAWave = currentWaveIndex >= 0;

        if (!_isInAArena || !_isInAWave)
        {
            if (_spawnedEnemies.Count > 0) _spawnedEnemies.Clear();
            return;
        }

        CalculateWaveIntensityAndRemainingEnemies();
        
        TimerCalculationPerformancePlayer();
        CalculatePlayerPerformance();
        
        CompareIntensityAndNextWaveValue();
    }

    /// <summary>
    /// Reset all the necessary script variables.
    /// </summary>
    private void ResetVariables()
    {
        playerPerformance = 0f;
        currentWaveIntensity = 0;
        dynamicNextWaveValue = 0f;
        _lastKilledEnemiesValue = 0f;
    }

    private bool CanGoToNextWave() => currentWaveIndex < GetActualArenaData().arenaWaves.Length - 1;

    private void OnGUI()
    {
        if (!DEBUG) return;

        // Set up GUI style for the text
        GUIStyle style = new GUIStyle
        {
            fontSize = 18,
            normal =
            {
                textColor = Color.white
            }
        };

        // Set the position and size of the text
        // 50 each part
        // 30 each elements
        Rect playerPerf = new Rect(10, 10, 200, 50);
        Rect playerPerfDelayCompar = new Rect(10, 40, 200, 50);
        
        Rect arenaIndex = new Rect(10, 90, 200, 50);
        Rect waveIndex = new Rect(10, 120, 200, 50);
        Rect waveIntensity = new Rect(10, 150, 200, 50);
        
        Rect dynamicNextWValue = new Rect(10, 200, 200, 50);
        
        Rect remainEnemies = new Rect(10, 250, 200, 50);
        
        Rect lastKilled = new Rect(10, 300, 200, 50);
        
        Rect isInArena = new Rect(10, 350, 200, 50);
        Rect isInWave = new Rect(10, 380, 200, 50);
        
        Rect finishedSpawn = new Rect(10, 430, 200, 50);
        
        Rect arenaFinished = new Rect(10, 480, 200, 50);
        
        Rect currentWavePerf = new Rect(10, 530, 200, 50);
        Rect currentWaveNextWValue = new Rect(10, 560, 200, 50);
        Rect currentWaveEnemyCount = new Rect(10, 590, 200, 50);

        // Display the text on the screen
        GUI.Label(playerPerf, $"Player Performance : {playerPerformance}", style);
        GUI.Label(playerPerfDelayCompar, $"Player Performance Delay Compar. : {playerPerformanceComparisonDelay}", style);
        
        GUI.Label(arenaIndex, $"Current Arena Index : {currentArenaIndex}", style);
        GUI.Label(waveIndex, $"Current Wave Index : {currentWaveIndex}", style);
        GUI.Label(waveIntensity, $"Current Wave Intensity : {currentWaveIntensity}", style);
        
        GUI.Label(dynamicNextWValue, $"Dynamic NextWaveValue : {dynamicNextWaveValue}", style);
        
        GUI.Label(remainEnemies, $"Remaining Enemies : {_currentRemainingEnemies}", style);
        
        GUI.Label(lastKilled, $"Last Killed Enemies Value : {_lastKilledEnemiesValue}", style);
        
        GUI.Label(isInArena, $"Is In Arena ? : {_isInAArena}", BoolStyle(_isInAArena));
        GUI.Label(isInWave, $"Is In Wave ? : {_isInAWave}", BoolStyle(_isInAWave));
        
        GUI.Label(finishedSpawn, $"Has Finished Spawned Enemies ? : {_hasFinishSpawningEnemies}", BoolStyle(_hasFinishSpawningEnemies));
        
        GUI.Label(arenaFinished, $"Current Arena Finished ? : {_currentArenaFinished}", BoolStyle(_currentArenaFinished));

        if (GetActualWave() is not null)
        {
            GUI.Label(currentWavePerf, $"Current Wave Perf. Ref. : {GetActualWave().performanceReference}", style);
            GUI.Label(currentWaveNextWValue, $"Current Wave NextWaveValue : {GetActualWave().nextWaveValue}", style);
            GUI.Label(currentWaveEnemyCount, $"Current Wave Enemies Amount: {GetActualWave().enemiesToSpawn.Length}", style);
        }
    }
    
    GUIStyle BoolStyle(bool value)
    {
        var style = new GUIStyle
        {
            fontSize = 24,
            normal =
            {
                textColor = value ? Color.green : Color.red
            }
        };
        return style;
    }
}
