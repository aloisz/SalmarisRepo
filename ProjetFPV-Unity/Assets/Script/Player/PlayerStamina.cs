using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStamina : GenericSingletonClass<PlayerStamina>
{
    public float staminaValue;

    [SerializeField] private int numberOfSteps;
    public bool HasEnoughStamina(int numberOfStep)
    {
        return staminaValue - ((100f / numberOfSteps) * numberOfStep) >= 0f;
    }

    public void ConsumeStaminaStep(int amount)
    {
        staminaValue = Mathf.Clamp(staminaValue - amount, 0, 100f);

        HUD.Instance.PlayDashVFX();
    }
    
    public void GenerateStaminaStep(float amount)
    {
        var value = amount * Time.deltaTime;
        staminaValue = Mathf.Clamp(staminaValue + value, 0, 100f);
    }
}
