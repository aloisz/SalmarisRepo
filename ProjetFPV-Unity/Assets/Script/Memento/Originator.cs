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
        return new Memento(PlayerController.Instance.transform);
    }

    public void Restore(Memento memento)
    {
        //Player
        PlayerController.Instance.transform.position = new Vector3(memento.x, memento.y, memento.z);
    }
}
