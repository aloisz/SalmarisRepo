using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class AsyncManager : MonoBehaviour
{
    [Header("Menu Screens")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject menuToQuit;

    [Header("Slider")]
    [SerializeField] private Image loadingSlider;

    public void LoadLevelButton(string sceneToTravel)
    {
        menuToQuit.SetActive(false);
        loadingScreen.SetActive(true);

        StartCoroutine(LoadLevelAsync(sceneToTravel));
    }

    IEnumerator LoadLevelAsync(string levelToLoad)
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelToLoad);
        while (!loadOperation.isDone)
        {
            float progressValue = Mathf.Clamp01(loadOperation.progress / 0.99f);
            loadingSlider.fillAmount = progressValue;
            yield return null;
        }
    }
}
