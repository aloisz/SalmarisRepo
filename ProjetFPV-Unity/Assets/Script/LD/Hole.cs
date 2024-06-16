using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hole : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<IDamage>() != null && !other.CompareTag("Player"))
        {
            other.GetComponent<IDamage>().Hit(999);
        }

        if (other.CompareTag("Player"))
        {
            PlayerHealth.Instance.DeathFromHole();
        }
    }
}
