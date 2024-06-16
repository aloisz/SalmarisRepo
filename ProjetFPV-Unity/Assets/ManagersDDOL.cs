using System;
using System.Collections;
using System.Collections.Generic;
using Script;
using UnityEngine;

public class ManagersDDOL : MonoBehaviour, IDestroyInstance
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    
    public void DestroyInstance()
    {
        Destroy(gameObject);
    }
}
