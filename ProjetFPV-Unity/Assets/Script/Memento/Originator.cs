using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AI;
using Player;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering.Universal;

public class Originator : MonoBehaviour
{
    public Memento Save()
    {
        List<bool> keysPickedUp = new List<bool>();
        foreach (Key b in FindObjectsOfType<Key>().ToList())
        {
            keysPickedUp.Add(b.isPickedUp);
        }
        
        //Player
        return new Memento(PlayerController.Instance.transform, PlayerHealth.Instance,
            WeaponState.Instance.barbatos, Director.Instance, keysPickedUp);
    }

    public void Restore(Memento memento)
    {
        ResetPlayer(memento);
        RestoreGun(memento);
        RestoreDirector(memento);
        SetAllDoorsState();
        ReturnAllEnemiesInPool();
        SetKeyData(memento);
        ReturnAllDecalsInPool();
    }

    /// <summary>
    /// RESTORE PLAYER PROPERTIES
    /// </summary>
    /// <param name="memento"></param>
    private void ResetPlayer(Memento memento)
    {
        // Position
        PlayerController.Instance.transform.position = memento.GetPlayerPosition();
        
        // Rotation
        PlayerController.Instance.transform.rotation = memento.GetPlayerRotation();
        
        //Health
        PlayerHealth.Instance._health = PlayerController.Instance.playerScriptable.maxPlayerHealth;
        
        // Shield
        PlayerHealth.Instance._shield = PlayerController.Instance.playerScriptable.maxPlayerShield;
        
        // Reset velocity
        PlayerController.Instance._rb.velocity = Vector3.zero;
    }

    /// <summary>
    /// RESTORE GUN PROPERTIES
    /// </summary>
    /// <param name="memento"></param>
    private void RestoreGun(Memento memento)
    {
        WeaponState.Instance.barbatos.so_Weapon.weaponMode[0] = memento.GetSOWeaponMode(0); // get the mode 0
        WeaponState.Instance.barbatos.so_Weapon.weaponMode[1] = memento.GetSOWeaponMode(1); // get the mode 1
    }

    /// <summary>
    /// RESTORE DIRECTOR PROPERTIES
    /// </summary>
    /// <param name="memento"></param>
    private void RestoreDirector(Memento memento)
    {
        Director director = Director.Instance;
        DirectorIntSection(memento, director);
        DirectorFloatSection(memento, director);
        DirectorBoolSection(memento, director);
    }

    private void DirectorIntSection(Memento memento, Director director)
    {
        director.totalIntensityValue = memento.GetTotalIntensityValue();
        director.totalIntensityValueLevel = memento.GetTotalIntensityValueLevel();
        director.numberOfDeath = memento.GetNumberOfDeath();
        director.currentArenaIndex = memento.GetCurrentArenaIndex();
        director.currentWaveIndex = memento.GetCurrentWaveIndex();
        director.playerOverPerformAmount = memento.GetPlayerOverPerfomAmount();
        director.currentRemainingEnemies = memento.GetCurrentRemainingEnemies();
        director.arenaAmount = memento.GetArenaAmount();
    }

    private void DirectorFloatSection(Memento memento, Director director)
    {
        director.currentWaveIntensity = memento.GetCurrentWaveIntensity();
        director.playerPerformance = memento.GetPlayerPerformance();
        director.dynamicNextWaveValue = memento.GetDynamicNextWaveValue();
        director.timerToCheckPlayerPerformance = memento.GetTimerToCheckPlayerPerformance();
        director.lastKilledEnemiesValue = memento.GetLastKilledEnemiesValue();
    }

    private void DirectorBoolSection(Memento memento, Director director)
    {
        director.currentArenaFinished = memento.GetCurrentArenaFinished();
        director.isInAArena = memento.GetIsInAArena();
        director.isInAWave = memento.GetIsInAWave();
        director.hasFinishSpawningEnemies = memento.GetHasFinishSpawningEnemies();
        director.hasStartedWave = memento.GetHasStartedWave();
    }
    
    private void SetAllDoorsState()
    {
        Door[] doors = FindObjectsOfType<Door>();
        foreach (Door d in doors)
        {
            if (d.isDeactivated) return;
            if(d.neededKey.isPickedUp) d.ActivateDoor();
            else d.DeactivateDoor(false);
        }
    }

    private void ReturnAllEnemiesInPool()
    {
        AI_Pawn[] pawns = FindObjectsOfType<AI_Pawn>();
        foreach (AI_Pawn p in pawns)
        {
            //TODO
            p.navMeshAgent.enabled = false;
            Pooling.Instance.DePop(p.so_IA.poolingName, p.gameObject);
        }
    }

    private void ReturnAllDecalsInPool()
    {
        DecalProjector[] decalProjectors = FindObjectsOfType<DecalProjector>();
        foreach (DecalProjector dp in decalProjectors)
        {
            Destroy(dp);
        }
    }
    
    private void SetKeyData(Memento memento)
    {
        int i = 0;
        foreach (Key k in FindObjectsOfType<Key>())
        {
            k.SetKeyData(memento.GetKeysPickedUp()[i]);
        }
    }
}
