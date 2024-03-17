using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStamina : GenericSingletonClass<PlayerStamina>
{
    public float staminaValue;

    [SerializeField] private int numberOfSteps;
    [SerializeField] private Image chargeUIDisplay;
    [SerializeField] private TextMeshProUGUI chargeText;
    
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

    /// <summary>
    /// Update the Stamina UI.
    /// </summary>
    private void UpdateStaminaUI()
    {
        chargeUIDisplay.fillAmount = staminaValue / 100f;
        chargeText.text = ((int)staminaValue).ToString();
    }
}
