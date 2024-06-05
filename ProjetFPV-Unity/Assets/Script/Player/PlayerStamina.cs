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

    public void ConsumeStaminaStep(int numberOfStep)
    {
        float removedValue = staminaValue - ((100f / numberOfSteps) * numberOfStep);
        staminaValue = Mathf.Clamp(removedValue, 0, 100f);
        
        if (Mathf.RoundToInt(staminaValue) <= 33)
        {
            HUD.Instance.PlayDashVFX(0);
        }

        if (Mathf.RoundToInt(staminaValue) is > 33 and < 66)
        {
            HUD.Instance.PlayDashVFX(1);
        }
        
        if (Mathf.RoundToInt(staminaValue) is > 66 and < 100)
        {
            HUD.Instance.PlayDashVFX(2);
        }
    }
    
    public void GenerateStaminaStep(float numberOfStep)
    {
        staminaValue = Mathf.Clamp(staminaValue + ((100f / numberOfSteps) * numberOfStep), 0, 100f);
    }
}
