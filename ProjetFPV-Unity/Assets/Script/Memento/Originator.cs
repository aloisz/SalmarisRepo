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
        return new Memento(PlayerController.Instance.transform, PlayerHealth.Instance);
    }

    public void Restore(Memento memento)
    {
        ResetPlayer(memento);
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
    }
}
