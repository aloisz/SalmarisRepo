using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerToUI : MonoBehaviour
{
    private void Start()
    {
        PlayerHealth.Instance.onHit += UpdateHealthShieldBars;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void UpdateHealthShieldBars()
    {
        
    }
}
