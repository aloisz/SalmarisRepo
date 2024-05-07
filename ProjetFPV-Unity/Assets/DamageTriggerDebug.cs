using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTriggerDebug : MonoBehaviour
{
   private float timer;
   
   private void OnTriggerEnter(Collider other)
   {
      if (other.CompareTag("Player"))
      {
         timer = 2f;
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
            timer = 2f;
         }
      }
   }
}
