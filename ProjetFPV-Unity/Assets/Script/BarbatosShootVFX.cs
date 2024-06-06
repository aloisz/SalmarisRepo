using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BarbatosShootVFX : MonoBehaviour
{
    [SerializeField] List<Color> colors = new List<Color>();
    [SerializeField] GameObject[] vfx;
    
    public void SpawnShootVFX(Color32 color, Vector3 pos, Vector3 normal)
    {
        var c = FindClosestColor(color, colors);
        var index = colors.IndexOf(c);
        var spawnedObject = Instantiate(vfx[index], pos, Quaternion.identity);
        spawnedObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);
    }
    
    public void SpawnShootVFX(int index, Vector3 pos, Vector3 normal)
    {
        var spawnedObject = Instantiate(vfx[index], pos, Quaternion.identity);
        spawnedObject.transform.rotation = Quaternion.FromToRotation(Vector3.forward, normal);
    }
    
    private Color FindClosestColor(Color targetColor, List<Color> colorList)
    {
        Color closestColor = colorList[0];
        float smallestDistance = ColorDistance(targetColor, closestColor);

        foreach (Color color in colorList)
        {
            float distance = ColorDistance(targetColor, color);
            if (distance < smallestDistance)
            {
                smallestDistance = distance;
                closestColor = color;
            }
        }

        return closestColor;
    }
    
    private static float ColorDistance(Color color1, Color color2)
    {
        float rDiff = color1.r - color2.r;
        float gDiff = color1.g - color2.g;
        float bDiff = color1.b - color2.b;

        return Mathf.Sqrt(rDiff * rDiff + gDiff * gDiff + bDiff * bDiff);
    }
}
