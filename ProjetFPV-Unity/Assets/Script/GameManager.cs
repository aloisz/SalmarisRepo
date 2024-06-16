using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using AI;
using NaughtyAttributes;
using Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class GameManager : GenericSingletonClass<GameManager>
{
    public int currentLevelIndex;
    public int currentCheckpointIndex;
    
    public Action OnLevelCompleted;

    [Header("LevelPlayersPosition")] [SerializeField]
    private LevelPlayersPosition levelPlayersPositions;

    public int globalScore;

    public override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
    
    public void LevelFinished()
    {
        OnLevelCompleted?.Invoke();
        Debug.Log("Level Finished");

        currentLevelIndex++;
        
        MusicManager.Instance.ChangeMusicPlayed(Music.FinNiveau, 1f, 0.25f);
        StartCoroutine(VoicelineManager.Instance.CallLevelOneFinishedVoiceLine());
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

    [Button("ChangeScene")]
    public void ChangeLevel()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(0);
        AsyncWaitForLoadingScene(asyncLoad);
    }


    async void AsyncWaitForLoadingScene(AsyncOperation asyncLoad)
    {
        Debug.Log("begin Async");
        while (!asyncLoad.isDone)
        {
            await Task.Yield();
            Debug.Log("Waiting async");
        }

        PlayerController.Instance.transform.position = levelPlayersPositions.levels[1].positionToSpawn;
        PlayerController.Instance.transform.eulerAngles = levelPlayersPositions.levels[1].directionToLook;
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


[System.Serializable]
public class LevelPlayersPosition
{
    public List<Level> levels;
}

[System.Serializable]
public class Level
{
    public Vector3 positionToSpawn;
    public Vector3 directionToLook;
}


