using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class Memento
{
    public float x;
    public float y;
    public float z;
    
    // Constructor
    public Memento(Transform playerTransform)
    {
        this.x = playerTransform.transform.position.x;
        this.y = playerTransform.transform.position.y;
        this.z = playerTransform.transform.position.z;
    }
}

