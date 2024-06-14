using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CareTaker : GenericSingletonClass<CareTaker> 
{
    [SerializeField] private Originator originator;

    private Memento savedMemento;

    [ContextMenu("Save State")]
    public void SaveGameState()
    {
        Debug.Log("Save");
        this.savedMemento = originator.Save();
    }

    [ContextMenu("Restore State")]
    public void LoadGameState()
    {
        Debug.Log("Restore");
        originator.Restore(savedMemento);
    }
}
