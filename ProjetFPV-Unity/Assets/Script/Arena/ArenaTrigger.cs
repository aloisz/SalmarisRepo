using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(BoxCollider))]
[ExecuteAlways]
public class ArenaTrigger : MonoBehaviour
{
    public int arenaID;
    [Expandable] public ArenaData arenaData;
    
    public List<EnemyPositionData> enemiesPositions = new List<EnemyPositionData>();
    [SerializeField] private List<Transform> waves = new List<Transform>();
    
    private BoxCollider _box;

    private void Awake()
    {
        _box ??= GetComponent<BoxCollider>();
        
        if (!_box.isTrigger)
        {
            _box.isTrigger = true;
            Debug.LogError($"{gameObject.name} has a collider who isn't in trigger. The attached {this} script need it to be trigger to work properly. " +
                             $"The collider is auto-set to trigger but consume resources on play.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //if the player enter in the arena trigger and start a new arena if the current arena ID is inferior to this one.
        //Its mean that this arena is the next one.
        if (other.CompareTag("Player") && Director.Instance.currentArenaIndex < arenaID)
        {
            //Start new wave in the director.
            Director.Instance.EnteringNewArena(arenaID);
            
            Debug.Log(other);
        }
    }

    public void SetupWaveEnemiesPosition()
    {
        if (waves.Count < transform.childCount) throw new Exception($"No enough Wave's Transform added to the list of {this}");
        
        enemiesPositions.Clear();
        
        int j = 0;
        foreach (Transform t in waves)
        {
            if (arenaData is null)
            {
                throw new Exception($"Cannot set waves enemies positions from {this} because no ArenaData is referenced.");
                return;
            }

            if (arenaData.arenaWaves.Length <= 0)
            {
                throw new Exception($"Cannot set waves enemies positions from {this} because no Waves was found for the Arena N°{arenaID}.");
                return;
            }
            var arenaWaves = arenaData.arenaWaves;
            

            //Setup the enemy positions list
            var x = new List<Vector3>();
            for (var k = 0; k < 10; k++) x.Add(Vector3.zero);
            enemiesPositions.Add(new EnemyPositionData()
            {
                positions = x
            });
            
            int i = 0;
            foreach (var e in arenaWaves[j].enemiesToSpawn)
            {
                if (t.childCount == 0)
                    throw new Exception($"No child found in the {t} Wave's Transform, in {this}");
                    
                //e.worldPosition = t.GetChild(i).position;
                enemiesPositions[j].positions[i] = t.GetChild(i).position;
                
                t.GetChild(i).gameObject.name = $"{Enum.GetName(typeof(EnemyToSpawn.EnemyKeys), e.enemyKey)}_{i}";
                
                #if UNITY_EDITOR
                Helper.SetupIconFromEnemyType(t.GetChild(i).gameObject, e.enemyKey);
                #endif

                i++;
            }
            
            Debug.Log($"Successfully set enemies positions for every waves of Arena N°{arenaID}.");
            
            j++;
        }
    }

    private void OnDrawGizmos()
    {
        _box = GetComponent<BoxCollider>();
        if (!_box) return;
        
        var color = new Color32(120, 10, 250, 255);
        Helper.DrawBoxCollider(color, transform, _box, 0.15f);

        var style = new GUIStyle()
        {
            fontSize = 40,
            alignment = TextAnchor.MiddleCenter,
            normal = new GUIStyleState()
            {
                textColor = new Color32(120, 10, 250, 255)
            }
        };
        
        #if UNITY_EDITOR
        Handles.Label(transform.position + new Vector3(0,_box.size.y / 2f,0), $"Arena N°{arenaID}", style);
        #endif
    }
}

[Serializable]
public class EnemyPositionData
{
    public List<Vector3> positions;
}
