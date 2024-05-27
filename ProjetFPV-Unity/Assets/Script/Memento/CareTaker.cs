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
        this.savedMemento = originator.Save();
        Debug.Log("SAVE");
    }

    [ContextMenu("Restore State")]
    public void LoadGameState()
    {
        originator.Restore(savedMemento);
        Debug.Log("RESTORE");
    }
}
