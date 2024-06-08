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
    
    
    public int avgFrameRate;
    public void Update ()
    {
        float current = 0;
        current = Time.frameCount / Time.time;
        avgFrameRate = (int)current;
    }
    
    private void OnGUI()
    {
        GUIStyle font = new GUIStyle();
        font.fontSize = 50;
        font.fontStyle = FontStyle.Bold;
        //font.font.material.color = Color.white;
        GUI.Label(new Rect(5, 40, 100, 25), "FPS: " + Mathf.Round(avgFrameRate), font);
    }
}
