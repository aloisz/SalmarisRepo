using System;
using AI;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Arena Data/ArenaData", fileName = "new ArenaData")]
public class ArenaData : ScriptableObject
{
    public ArenaWave[] arenaWaves;
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
