using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTriggerDebug : MonoBehaviour
{
   [SerializeField] private float dmgIntervallDelay;
   private float timer;
   
   private void OnTriggerEnter(Collider other)
   {
      if (other.CompareTag("Player"))
      {
         timer = dmgIntervallDelay;
      }
   }
   
   private void OnTriggerStay(Collider other)
   {
      if (other.CompareTag("Player"))
      {
         timer.DecreaseTimerIfPositive();
         if (timer < 0.05f)
         {
            PlayerHealth.Instance.Hit(10f);
            PlayerHealth.Instance.lastEnemyPosition = transform.position;
            timer = dmgIntervallDelay;
         }
      }
   }
}
