using System;
using System.Collections;
using System.Collections.Generic;
using AI;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class GameManager : GenericSingletonClass<GameManager>
{
    public int currentLevelIndex;
    public int currentCheckpointIndex;
    
    public Action OnLevelCompleted;

    public int globalScore;

    public void LevelFinished()
    {
        OnLevelCompleted?.Invoke();
        Debug.Log("Level Finished");

        currentLevelIndex++;
    }
    
    
    private  int avgFrameRate;
    private int frameRate;
    private float ms;
    public void Update ()
    {
        float current = 0;
        current = Time.frameCount / Time.time;
        avgFrameRate = (int)current;
        
        frameRate = (int) (1f / Time.deltaTime);
        
        ms = Time.deltaTime * 1000;
    }
    
    private void OnGUI()
    {
        GUIStyle font = new GUIStyle();
        font.fontSize = 50;
        font.fontStyle = FontStyle.Bold;
        //font.font.material.color = Color.white;
        GUI.Label(new Rect(5, 40, 100, 25), "Average FPS: " + Mathf.Round(avgFrameRate), font);
        
        GUI.Label(new Rect(5, 90, 100, 25), "FPS: " + Mathf.Round(frameRate), font);
        
        GUI.Label(new Rect(5, 140, 100, 25), "ms: " + Mathf.Round(ms), font);
    }
}
