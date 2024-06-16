using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AI;
using MyAudio;
using Player;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class Director : GenericSingletonClass<Director>
{
    [SerializeField] private bool DEBUG_DRAW_GUI;
    [SerializeField] private bool DEBUG_DONT_NEED_PREVIOUS_ARENAS_CLEARED;
    
    public float levelTimer;
    
    [SerializeField] private float playerPerformanceComparisonDelay = 0.25f;
    [SerializeField] private List<ArenaTrigger> arenas = new List<ArenaTrigger>();
    
    public int totalIntensityValue;
    public int totalIntensityValueLevel;

    public int numberOfDeath;
    
    public int currentArenaIndex;
    public int currentWaveIndex;

    public int playerOverPerformAmount;

    //--------------------------------------------------------
    
    private List<AI_Pawn> _spawnedEnemies = new List<AI_Pawn>();

    [HideInInspector]public float currentWaveIntensity;
    [HideInInspector]public float playerPerformance;
    [HideInInspector]public float dynamicNextWaveValue;
    
    [HideInInspector]public int currentRemainingEnemies;
    [HideInInspector]public float timerToCheckPlayerPerformance;
    [HideInInspector]public float lastKilledEnemiesValue;

    [HideInInspector]public bool currentArenaFinished = true;

    [HideInInspector]public bool isInAArena;
    [HideInInspector]public bool isInAWave;
    [HideInInspector]public bool hasFinishSpawningEnemies;
    [HideInInspector]public bool hasStartedWave;

    [HideInInspector]public int arenaAmount;

    //---------------------------------------


    private void Start()
    {
        EnteringNewLevel();
        PlayerHealth.Instance.onDeath = () => numberOfDeath += 1;
    }
    
    private void EnteringNewLevel()
    {
        foreach (ArenaTrigger at in FindObjectsOfType<ArenaTrigger>()) arenaAmount++;
        MusicManager.Instance.ChangeMusicPlayed(Music.Intro, 0f, 0.1f, 0.1f);
    }

    /// <summary>
    /// Function to start notify the Director that the player is inside an Arena.
    /// </summary>
    /// <param name="arenaID">The Arena ID to start wave of.</param>
    public void EnteringNewArena(int arenaID)
    {
        if (!currentArenaFinished || arenaID <= currentArenaIndex || (!DEBUG_DONT_NEED_PREVIOUS_ARENAS_CLEARED && arenaID != currentArenaIndex + 1)) return;

        currentRemainingEnemies = 0;

        //when entering in the arena, notify the director if the new arena ID.
        currentArenaIndex = arenaID;
        
        if (GetActualArenaTrigger().arenaUnlockedDoors.Length > 0)
        {
            foreach(Door d in GetActualArenaTrigger().arenaUnlockedDoors) if(d.neededKey is null) d.ActivateDoor();
        }
        
        //start a new wave.
        if(CanGoToNextWave()) StartCoroutine(nameof(StartNewWave));

        if (GameManager.Instance.currentLevelIndex == 0 && currentArenaIndex == 0)
        {
            VoicelineManager.Instance.CallFirstArenaDialogues();
        }
        
        if (currentArenaIndex == 0)
        {
            MusicManager.Instance.ChangeMusicPlayed(Music.Start, 0f, 0.25f, 0f, true);
            MusicManager.Instance.ChangeMusicPlayed
                (Music.Fight, 1f, 0.25f, 2.8f, true);
        }
        else if(currentArenaIndex == 6)
        {
            MusicManager.Instance.ChangeMusicPlayed(Music.StartFinalFight, 0f, 0.25f, 0f, true);
            MusicManager.Instance.ChangeMusicPlayed
                (Music.FinalFight, 1f, 0.25f, 14f, true);
        }
        else
        {
            MusicManager.Instance.ChangeMusicPlayed
                (Music.Fight, 1f, 0.25f, 0f, true);
        }
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

        currentArenaFinished = false;
        hasStartedWave = true;
        hasFinishSpawningEnemies = false;

        //set the (dynamic)NextWaveValue to the NextWaveValue reference in the wave data.
        dynamicNextWaveValue = GetActualWave().nextWaveValue;
        
        //if the wave doesn't contain any mob to spawn...
        if (GetActualWave().enemiesToSpawn.Length <= 0)
            throw new Exception($"{GetActualWave()} doesn't have any enemy to spawn.");
        

        yield return new WaitForSeconds(GetActualWave().delayBeforeSpawn);
        
        StopCoroutine(nameof(SpawnSecurityCheck));
        StartCoroutine(nameof(SpawnSecurityCheck));

        StartCoroutine(nameof(SpawnEnemies));
    }

    /// <summary>
    /// Routine that handles the mob spawning.
    /// </summary>
    /// <returns></returns>
    private IEnumerator SpawnEnemies()
    {
        int i = 0;
        hasFinishSpawningEnemies = false;

        foreach (EnemyToSpawn e in GetActualWave().enemiesToSpawn)
        {
            GameObject mob = Pooling.Instance.Pop(Enum.GetName(typeof(EnemyToSpawn.EnemyKeys), e.enemyKey));
            
            mob.name = $"{Enum.GetName(typeof(EnemyToSpawn.EnemyKeys), e.enemyKey)}<br>Arena : {currentArenaIndex}<br>Wave : {currentWaveIndex}";
            
            AI_Pawn p = mob.GetComponent<AI_Pawn>();
            RaycastHit hit;
            Physics.Raycast(GetActualArenaTrigger().enemiesPositions[currentWaveIndex].positions[i],
                Vector3.down, out hit, 500f);
            Debug.DrawLine(GetActualArenaTrigger().enemiesPositions[currentWaveIndex].positions[i], hit.point, Color.green, 50f);
            
            /*NavMeshHit navMeshHit;
            if (NavMesh.SamplePosition(hit.point, out navMeshHit, 1.0f, NavMesh.AllAreas))
            {
                mob.transform.position = navMeshHit.position + new Vector3(0,2,0);
            }*/

            mob.transform.position = hit.point + new Vector3(0,2,0);
            
            p.SpawnVFX();
            _spawnedEnemies.Add(p);
            p.ResetAgent(true);

            currentRemainingEnemies += 1;
            currentWaveIntensity += p.enemyWeight;

            yield return new WaitForSeconds(GetActualWave().delayBetweenEachEnemySpawn);

            i++;
        }
        
        hasFinishSpawningEnemies = true;
        hasStartedWave = false;
    }

    /// <summary>
    /// Compare the intensity of the wave, and the dynamicNextWaveValue for notify if the player is performing too much.
    /// If the dynamicNextWaveValue go too high too fast, the currentWaveIntensity will be inferior, so start the new wave.
    /// </summary>
    private void CompareIntensityAndNextWaveValue()
    {
        if (currentWaveIntensity < dynamicNextWaveValue && hasFinishSpawningEnemies && !hasStartedWave)
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
        if (playerPerformance > GetActualWave().performanceReference)
        {
            dynamicNextWaveValue += GetActualWave().nextWaveValueAddedValue;
            playerOverPerformAmount++;
        }
    }

    /// <summary>
    /// Calculate the player's performance, based on the last killed enemies, in timerToCheckPlayerPerformance seconds.
    /// </summary>
    private void CalculatePlayerPerformance()
    {
        playerPerformance = lastKilledEnemiesValue;
    }

    /// <summary>
    /// Timer that handle the verification of the player's performance, if the timer runs out, start comparing the player's perf with
    /// the current wave reference one. Reset the timer and the last killed enemies.
    /// </summary>
    private void TimerCalculationPerformancePlayer()
    {
        timerToCheckPlayerPerformance.DecreaseTimerIfPositive();
        
        if (timerToCheckPlayerPerformance <= 0)
        {
            timerToCheckPlayerPerformance = playerPerformanceComparisonDelay;
            
            ComparePlayerPerfAndReferencePerf();
            lastKilledEnemiesValue = 0;
        }
    }

    /// <summary>
    /// Function called when an enemy is dead. It will try to add the weight to the last killed enemy.
    /// </summary>
    /// <param name="value">Enemy's weight.</param>
    public void TryAddingValueFromLastKilledEnemy(float value)
    {
        lastKilledEnemiesValue += value;
        currentRemainingEnemies--;
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
        currentArenaFinished = true;
        GetActualArenaTrigger().isCompleted = true;
        
        StopCoroutine(nameof(SpawnSecurityCheck));
        
        hasStartedWave = false;
        currentWaveIndex = -1;

        playerOverPerformAmount = 0;
        
        totalIntensityValue = ReturnTotalIntensityArenaValue();
        totalIntensityValueLevel += totalIntensityValue;

        ScoringSystem.Instance.ScoreEndArena();

        VoicelineManager.Instance.CallArenaEndDialogues();
        
        StartCoroutine(DelayDoorOpen(
            currentArenaIndex == 0 && GameManager.Instance.currentLevelIndex == 0 ? 16f : 0f));

        if (GetActualArenaData().shouldSpawnShopAtTheEnd)
        {
            if (currentArenaIndex == 0 && GameManager.Instance.currentLevelIndex == 0)
            {
                StartCoroutine(UpgradeModule.Instance.InitModule(GetActualArenaData().shopOrbitalPosition, 
                    GetActualArenaData().possibleUpgrades,12f));
                VoicelineManager.Instance.CallFirstShopVoiceLine();
            }
            else
            {
                StartCoroutine(UpgradeModule.Instance.InitModule(GetActualArenaData().shopOrbitalPosition, 
                    GetActualArenaData().possibleUpgrades));
                VoicelineManager.Instance.CallShopVoiceLine();
            }
            MusicManager.Instance.ManageActualSoundVolume(0.025f);
        }
        
        if(currentArenaIndex == arenaAmount - 1) GameManager.Instance.LevelFinished();
        else MusicManager.Instance.ChangeMusicPlayed(Music.Ambiance, 1f, 0.25f, 0.1f);
    }

    IEnumerator DelayDoorOpen(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (GetActualArenaTrigger().arenaUnlockedDoors.Length > 0)
        {
            foreach (Door d in GetActualArenaTrigger().arenaUnlockedDoors)
            {
                d.DeactivateDoor(true);
                d.ActivateLockedDoor();
            }
        }
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
        
        isInAArena = !currentArenaFinished && isInAWave;
        isInAWave = currentWaveIndex >= 0;

        if (!isInAArena || !isInAWave)
        {
            if (_spawnedEnemies.Count > 0) _spawnedEnemies.Clear();
            return;
        }
        
        if (currentRemainingEnemies <= 0 && !CanGoToNextWave() && !currentArenaFinished && hasFinishSpawningEnemies)
        {
            NotifyArenaCompleted();
        }
        else
        {
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
        playerPerformance = 0f;
        currentWaveIntensity = 0;
        dynamicNextWaveValue = 0f;
        lastKilledEnemiesValue = 0f;
    }

    private bool CanGoToNextWave() => currentWaveIndex != GetActualArenaData().arenaWaves.Length - 1;

    IEnumerator SpawnSecurityCheck()
    {
        yield return new WaitForSeconds(5f);
        
        foreach (AI_Pawn aiPawn in _spawnedEnemies)
        {
            if (Vector3.Distance(aiPawn.transform.position, GetActualArenaTrigger().transform.position) > 500f)
            {
                Debug.Log($"<color=red><b>Killed a mob</b></color> at {aiPawn.transform.position}.");
                aiPawn.Hit(999);
            }
        }

        StartCoroutine(SpawnSecurityCheck());
    }

    private void OnGUI()
    {
        if (!DEBUG_DRAW_GUI) return;

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
        
        GUI.Label(remainEnemies, $"Remaining Enemies : {currentRemainingEnemies}", style);
        
        GUI.Label(lastKilled, $"Last Killed Enemies Value : {lastKilledEnemiesValue}", style);

        if (GetActualArenaData())
        {
            GUI.Label(isInArena, $"Can Go Next Wave ? : {CanGoToNextWave()}", BoolStyle(CanGoToNextWave()));
            GUI.Label(isInWave, $"Arena Waves Amount : {GetActualArenaData().arenaWaves.Length}", style);
        }

        GUI.Label(finishedSpawn, $"Has Finished Spawned Enemies ? : {hasFinishSpawningEnemies}", BoolStyle(hasFinishSpawningEnemies));
        
        GUI.Label(arenaFinished, $"Current Arena Finished ? : {currentArenaFinished}", BoolStyle(currentArenaFinished));

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
