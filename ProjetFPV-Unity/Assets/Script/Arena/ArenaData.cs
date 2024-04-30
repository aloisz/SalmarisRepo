using System;
using AI;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Arena Data/ArenaData", fileName = "new ArenaData")]
public class ArenaData : ScriptableObject
{
    public ArenaWave[] arenaWaves;
    public bool shouldSpawnShopAtTheEnd;
    [InfoBox("Orbital Position is the position where the module will be in the sky. Example, you want it to land at 0,0,0, so just put 300 in Y value.")]
    public Vector3 shopOrbitalPosition;
}

[Serializable]
public class ArenaWave
{
    public EnemyToSpawn[] enemiesToSpawn;
    
    public float performanceReference;

    public float nextWaveValue;
    public float nextWaveValueAddedValue;
    
    public float delayBetweenEachEnemySpawn;

    public float delayBeforeSpawn;
}

[Serializable]
public class EnemyToSpawn
{
    public EnemyKeys enemyKey;
    public Vector3 worldPosition;
    
    public enum EnemyKeys
    {
        TrashMob,
        AirSack
    }
}
