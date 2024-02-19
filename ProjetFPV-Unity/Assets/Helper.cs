using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helper
{
    public static Vector2 ConvertTo4Dir(Vector2 inputVector)
    {
        //checks if the x or y axis is dominant
        bool isAxisVertical = Mathf.Abs(inputVector.y) >= Mathf.Abs(inputVector.x);
        bool isAxisPositive = isAxisVertical ? inputVector.y >= 0 : inputVector.x >= 0;
        if (isAxisVertical) return isAxisPositive ? Vector2.up : Vector2.down;
        else return isAxisPositive ? Vector2.right : Vector2.left;
    }
    
    public static void DecreaseTimerIfPositive(this ref float f)
    {
        if (f > 0) f -= Time.deltaTime;
        if (f < 0) f = 0;
    }
    
    public static void DecreaseTimerIfPositiveNoTimeScale(this ref float f)
    {
        if (f > 0) f -= Time.unscaledDeltaTime;
        if (f < 0) f = 0;
    }
}
