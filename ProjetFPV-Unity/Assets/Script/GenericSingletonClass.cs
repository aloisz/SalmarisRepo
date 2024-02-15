using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericSingletonClass<T> : MonoBehaviour where T: Component
{
    public static T Instance;

    public virtual void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this as T;
    }
}
