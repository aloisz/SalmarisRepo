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

    public static Vector3 ConvertToV3Int(Vector3 v)
    {
        var vector = new Vector3(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));
        return vector;
    }

    public static Vector3 ReturnDirFromIndex(int index)
    {
        Vector3 value = Vector3.zero;
        switch (index)
        {
            case 0: value = Vector3.right; break;
            case 1: value = Vector3.left; break;
            case 2: value = Vector3.forward; break;
            case 3: value = Vector3.back; break;
        }
        return value;
    }
    
    public static Vector3 ReturnDirFromTransform(int index, Transform t)
    {
        Vector3 value = Vector3.zero;
        switch (index)
        {
            case 0: value = t.right; break;
            case 1: value = -t.right; break;
            case 2: value = t.forward; break;
            case 3: value = -t.forward; break;
        }
        return value;
    }

    /*private static float timer = 0;
    public static void TimerWait(float tickVerification, )
    {
        
    }*/
}
