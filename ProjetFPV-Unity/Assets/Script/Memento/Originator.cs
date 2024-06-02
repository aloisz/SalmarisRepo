using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AI;
using Player;
using UnityEngine;
using UnityEngine.Playables;

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

        int i = 0;
        foreach (Key k in FindObjectsOfType<Key>())
        {
            k.SetKeyData(memento.GetKeysPickedUp()[i]);
        }
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
        PlayerHealth.Instance._health = memento.GetPlayerHealth();
        
        // Shield
        PlayerHealth.Instance._shield = memento.GetPlayerShield();
        
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
        director.playerOverPerfomAmount = memento.GetPlayerOverPerfomAmount();
        director._currentRemainingEnemies = memento.GetCurrentRemainingEnemies();
        director._arenaAmount = memento.GetArenaAmount();
    }

    private void DirectorFloatSection(Memento memento, Director director)
    {
        director._currentWaveIntensity = memento.GetCurrentWaveIntensity();
        director._playerPerformance = memento.GetPlayerPerformance();
        director._dynamicNextWaveValue = memento.GetDynamicNextWaveValue();
        director._timerToCheckPlayerPerformance = memento.GetTimerToCheckPlayerPerformance();
        director._lastKilledEnemiesValue = memento.GetLastKilledEnemiesValue();
    }

    private void DirectorBoolSection(Memento memento, Director director)
    {
        director._currentArenaFinished = memento.GetCurrentArenaFinished();
        director._isInAArena = memento.GetIsInAArena();
        director._isInAWave = memento.GetIsInAWave();
        director._hasFinishSpawningEnemies = memento.GetHasFinishSpawningEnemies();
        director._hasStartedWave = memento.GetHasStartedWave();
    }
    
    private void SetAllDoorsState()
    {
        Door[] doors = FindObjectsOfType<Door>();
        foreach (Door d in doors)
        {
            if(d.neededKey != null && d.neededKey.isPickedUp) d.ActivateDoor();
            else d.DeactivateDoor();
        }
    }

    private void ReturnAllEnemiesInPool()
    {
        AI_Pawn[] pawns = FindObjectsOfType<AI_Pawn>();
        foreach(AI_Pawn p in pawns) p.DestroyLogic();
    }
}
