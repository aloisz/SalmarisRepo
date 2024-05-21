using System;
using System.Collections;
using System.Collections.Generic;
using AI;
using Player;
using UnityEngine;
using UnityEngine.Serialization;

public class Director : GenericSingletonClass<Director>
{
    public float levelTimer;
    
    [SerializeField] private bool DEBUG;
    [SerializeField] private float playerPerformanceComparisonDelay = 0.25f;
    [SerializeField] private List<ArenaTrigger> arenas = new List<ArenaTrigger>();

    public Action onArenaFinished;
    public int totalIntensityValue;
    public int totalIntensityValueLevel;

    public int numberOfDeath;
    
    public int currentArenaIndex;
    public int currentWaveIndex;

    public int playerOverPerfomAmount;

    //--------------------------------------------------------
    
    private List<AI_Pawn> _spawnedEnemies = new List<AI_Pawn>();

    private float _currentWaveIntensity;
    private float _playerPerformance;
    private float _dynamicNextWaveValue;
    
    private int _currentRemainingEnemies;
    private float _timerToCheckPlayerPerformance;
    private float _lastKilledEnemiesValue;

    private bool _currentArenaFinished = true;

    private bool _isInAArena;
    private bool _isInAWave;
    private bool _hasFinishSpawningEnemies;
    private bool _hasStartedWave;

    private int _arenaAmount;

    //---------------------------------------


    private void Start()
    {
        EnteringNewLevel();
        PlayerController.Instance.onDeath = () => numberOfDeath += 1;
    }

    private void EnteringNewLevel()
    {
        foreach (ArenaTrigger at in FindObjectsOfType<ArenaTrigger>()) _arenaAmount++;
    }

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
        
        if (GetActualArenaTrigger().arenaUnlockedDoors.Length > 0)
        {
            foreach(Door d in GetActualArenaTrigger().arenaUnlockedDoors) d.ActivateDoor();
        }
        
        //start a new wave.
        if(CanGoToNextWave()) StartCoroutine(nameof(StartNewWave));
    }

    /// <summary>
    /// Routine to start a new wave.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    IEnumerator StartNewWave()
    {
        //reset all variables.
        ResetWaveVariables();
        
        yield return new WaitUntil(() => !GetActualArenaTrigger().key || GetActualArenaTrigger().key.isPickedUp);
        
        currentWaveIndex++;
        
        Debug.Log(currentWaveIndex);
        Debug.Log(GetActualWave());

        _currentArenaFinished = false;
        _hasStartedWave = true;
        _hasFinishSpawningEnemies = false;

        //set the (dynamic)NextWaveValue to the NextWaveValue reference in the wave data.
        _dynamicNextWaveValue = GetActualWave().nextWaveValue;
        
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
            
            mob.transform.position = GetActualArenaTrigger().enemiesPositions[currentWaveIndex].positions[i];
            
            _spawnedEnemies.Add(p);

            yield return new WaitForSeconds(GetActualWave().delayBetweenEachEnemySpawn);

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
        _currentWaveIntensity = 0;
        _currentRemainingEnemies = 0;
        
        foreach (AI_Pawn ai in _spawnedEnemies)
        {
            if (ai.actualPawnHealth > 0)
            {
                //add every mob's weight to the starting wave intensity.
                _currentWaveIntensity += ai.enemyWeight;
                _currentRemainingEnemies += 1;
            }
        }
    }

    /// <summary>
    /// Compare the intensity of the wave, and the dynamicNextWaveValue for notify if the player is performing too much.
    /// If the dynamicNextWaveValue go too high too fast, the currentWaveIntensity will be inferior, so start the new wave.
    /// </summary>
    private void CompareIntensityAndNextWaveValue()
    {
        if (_currentWaveIntensity < _dynamicNextWaveValue && _hasFinishSpawningEnemies && !_hasStartedWave)
        {
            if(CanGoToNextWave()) StartCoroutine(nameof(StartNewWave));
        }
    }

    /// <summary>
    /// Compare the player's performance with the actual wave's performance reference.
    /// If the player's performance is higher at the comparison moment, then increment the dynamicNextWaveValue from the
    /// current wave's incrementation variable.
    /// </summary>
    private void ComparePlayerPerfAndReferencePerf()
    {
        if (_playerPerformance > GetActualWave().performanceReference)
        {
            _dynamicNextWaveValue += GetActualWave().nextWaveValueAddedValue;
            playerOverPerfomAmount++;
        }
    }

    /// <summary>
    /// Calculate the player's performance, based on the last killed enemies, in timerToCheckPlayerPerformance seconds.
    /// </summary>
    private void CalculatePlayerPerformance()
    {
        _playerPerformance = _lastKilledEnemiesValue;
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
        return GetActualArenaTrigger().arenaData;
    }

    private ArenaWave GetActualWave()
    {
        if (currentWaveIndex < 0) return null;
        return GetActualArenaData().arenaWaves[currentWaveIndex]; 
    }
    
    private ArenaTrigger GetActualArenaTrigger()
    {
        if (currentArenaIndex < 0) return null;
        return arenas[currentArenaIndex];
    }

    private void NotifyArenaCompleted()
    {
        _currentArenaFinished = true;
        _hasStartedWave = false;
        currentWaveIndex = -1;

        playerOverPerfomAmount = 0;
        
        totalIntensityValue = ReturnTotalIntensityArenaValue();
        totalIntensityValueLevel += totalIntensityValue;
        
        onArenaFinished.Invoke();
        
        if (GetActualArenaTrigger().arenaUnlockedDoors.Length > 0)
        {
            foreach(Door d in GetActualArenaTrigger().arenaUnlockedDoors) d.DeactivateDoor();
        }

        if(GetActualArenaData().shouldSpawnShopAtTheEnd) 
            UpgradeModule.Instance.InitModule(GetActualArenaData().shopOrbitalPosition, GetActualArenaData().possibleUpgrades);
        
        if(currentArenaIndex == _arenaAmount - 1) GameManager.Instance.LevelFinished();
    }

    private int ReturnTotalIntensityArenaValue()
    {
        float value = 0;
        foreach (AI_Pawn aiPawn in _spawnedEnemies)
        {
            value += aiPawn.enemyWeight;
        }
        return (int)value;
    }

    private void LevelTimer() => levelTimer += Time.deltaTime;
    
    private void Update()
    {
        LevelTimer();
        
        _isInAArena = !_currentArenaFinished && _isInAWave;
        _isInAWave = currentWaveIndex >= 0;

        if (!_isInAArena || !_isInAWave)
        {
            if (_spawnedEnemies.Count > 0) _spawnedEnemies.Clear();
            return;
        }
        
        if (_currentRemainingEnemies <= 0 && !CanGoToNextWave() && !_currentArenaFinished && _hasFinishSpawningEnemies)
        {
            NotifyArenaCompleted();
        }
        else
        {
            CalculateWaveIntensityAndRemainingEnemies();
        
            TimerCalculationPerformancePlayer();
            CalculatePlayerPerformance();
        
            CompareIntensityAndNextWaveValue();
        }
    }

    /// <summary>
    /// Reset all the necessary script variables.
    /// </summary>
    private void ResetWaveVariables()
    {
        _playerPerformance = 0f;
        _currentWaveIntensity = 0;
        _dynamicNextWaveValue = 0f;
        _lastKilledEnemiesValue = 0f;
    }

    private bool CanGoToNextWave() => currentWaveIndex != GetActualArenaData().arenaWaves.Length - 1;

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
        GUI.Label(playerPerf, $"Player Performance : {_playerPerformance}", style);
        GUI.Label(playerPerfDelayCompar, $"Player Performance Delay Compar. : {playerPerformanceComparisonDelay}", style);
        
        GUI.Label(arenaIndex, $"Current Arena Index : {currentArenaIndex}", style);
        GUI.Label(waveIndex, $"Current Wave Index : {currentWaveIndex}", style);
        GUI.Label(waveIntensity, $"Current Wave Intensity : {_currentWaveIntensity}", style);
        
        GUI.Label(dynamicNextWValue, $"Dynamic NextWaveValue : {_dynamicNextWaveValue}", style);
        
        GUI.Label(remainEnemies, $"Remaining Enemies : {_currentRemainingEnemies}", style);
        
        GUI.Label(lastKilled, $"Last Killed Enemies Value : {_lastKilledEnemiesValue}", style);

        if (GetActualArenaData())
        {
            GUI.Label(isInArena, $"Can Go Next Wave ? : {CanGoToNextWave()}", BoolStyle(CanGoToNextWave()));
            GUI.Label(isInWave, $"Arena Waves Amount : {GetActualArenaData().arenaWaves.Length}", style);
        }

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
