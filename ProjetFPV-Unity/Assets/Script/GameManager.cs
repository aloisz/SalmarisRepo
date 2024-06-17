using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AI;
using CameraBehavior;
using NaughtyAttributes;
using Player;
using Script;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : GenericSingletonClass<GameManager>, IDestroyInstance
{
    [SerializeField] private bool DebugFPS;
    
    public int currentLevelIndex;
    public int currentCheckpointIndex;

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
        ScoringSystem.Instance.ScoreEndLevel();

        currentLevelIndex++;
        
        MusicManager.Instance.ChangeMusicPlayed(Music.FinNiveau, 1f, 0.25f);
        VoicelineManager.Instance.CallLevelOneFinishedVoiceLine();
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

        if (Input.GetKeyDown(KeyCode.I))
        {
            ChangeLevel(0);
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            ChangeLevel(1);
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            ChangeLevel(2);
        }
    }

    [Button("ChangeScene 0")]
    public void ChangeLevel(int buildIndex)
    {
        //IDestroyInstance[] Interface = (IDestroyInstance[])FindObjectsOfType (typeof(IDestroyInstance));
        if (buildIndex == 0)
        {
            IDestroyInstance[] Interface = FindObjectsOfType<MonoBehaviour>().OfType<IDestroyInstance>().ToArray();
            foreach (IDestroyInstance toDestroyInstance in Interface) 
            {
                toDestroyInstance.DestroyInstance();
            }
        }
        
        Time.timeScale = 1;
        if(PauseMenu.Instance) PauseMenu.Instance.QuitPause();
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(buildIndex);
        AsyncWaitForLoadingScene(asyncLoad, buildIndex);
    }


    async void AsyncWaitForLoadingScene(AsyncOperation asyncLoad, int value)
    {
        Debug.Log("begin Async");
        while (!asyncLoad.isDone)
        {
            await Task.Yield();
            LoadingScreen.Instance.UpdateFiller(asyncLoad.progress);
            Debug.Log("Waiting async");
        }

        PlayerController.Instance.transform.position = levelPlayersPositions.levels[value].positionToSpawn;
        PlayerController.Instance.transform.eulerAngles = levelPlayersPositions.levels[value].directionToLook;

        CameraManager.Instance.transform.position = levelPlayersPositions.levels[value].cameraPos;
        CameraManager.Instance.transform.eulerAngles = levelPlayersPositions.levels[value].cameraRot;
        
        LoadingScreen.Instance.CloseLoading();
    }
    
    private void OnGUI()
    {
        if(!DebugFPS) return;
        GUIStyle font = new GUIStyle();
        font.fontSize = 50;
        font.fontStyle = FontStyle.Bold;
        //font.font.material.color = Color.white;
        GUI.Label(new Rect(5, 40, 100, 25), "Average FPS: " + Mathf.Round(avgFrameRate), font);
        
        GUI.Label(new Rect(5, 90, 100, 25), "FPS: " + Mathf.Round(frameRate), font);
        
        GUI.Label(new Rect(5, 140, 100, 25), "ms: " + Mathf.Round(ms), font);
    }

    public void DestroyInstance()
    {
        Destroy(gameObject);
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

    public Vector3 cameraPos;
    public Vector3 cameraRot;
}


