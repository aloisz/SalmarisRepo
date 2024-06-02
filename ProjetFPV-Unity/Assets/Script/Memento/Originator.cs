using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Player;
using UnityEngine;

public class Originator : MonoBehaviour
{
    public Memento Save()
    {
        //Player
        return new Memento(PlayerController.Instance.transform, PlayerHealth.Instance, WeaponState.Instance.barbatos);
    }

    public void Restore(Memento memento)
    {
        ResetPlayer(memento);
        RestoreGun(memento);
    }

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

    private void RestoreGun(Memento memento)
    {
        WeaponState.Instance.barbatos.so_Weapon.weaponMode[0] = memento.GetSOWeaponMode(0); // get the mode 0
        WeaponState.Instance.barbatos.so_Weapon.weaponMode[1] = memento.GetSOWeaponMode(1); // get the mode 1
    }
}
