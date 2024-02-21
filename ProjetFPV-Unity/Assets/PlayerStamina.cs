using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStamina : GenericSingletonClass<PlayerStamina>
{
    public float staminaValue;

    [SerializeField] private int numberOfSteps;
    [SerializeField] private Image chargeUIDisplay;
    
    public bool HasEnoughStamina(int numberOfStep)
    {
        return staminaValue - ((100f / numberOfSteps) * numberOfStep) >= 0f;
    }

    public void ConsumeStaminaStep(int numberOfStep)
    {
        staminaValue = Mathf.Clamp(staminaValue - ((100f / numberOfSteps) * numberOfStep), 0, 100f);
        UpdateStaminaUI();
    }
    
    public void GenerateStaminaStep(float numberOfStep)
    {
        staminaValue = Mathf.Clamp(staminaValue + ((100f / numberOfSteps) * numberOfStep), 0, 100f);
        UpdateStaminaUI();
    }

    private void UpdateStaminaUI()
    {
        chargeUIDisplay.fillAmount = staminaValue / 100f;
    }
}
