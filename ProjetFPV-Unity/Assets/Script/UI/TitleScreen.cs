using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TitleScreen : MonoBehaviour
{
    [SerializeField] private GameObject optionMenu;
    [SerializeField] private GameObject levelSelectionMenu;

    [SerializeField] private List<string> scenesLevels = new List<string>();
    
    [SerializeField] private AsyncManager asyncManager;

    public void OpenOptions() => optionMenu.SetActive(true);
    public void CloseOptions() => optionMenu.SetActive(false);
    public void Quit() => Application.Quit();
    
    public void OpenLevelSelection() => levelSelectionMenu.SetActive(true);
    public void CloseLevelSelection() => levelSelectionMenu.SetActive(false);

    public void PlayLevelOne() => asyncManager.LoadLevelButton(scenesLevels[0]);
    public void PlayLevelTwo() => asyncManager.LoadLevelButton(scenesLevels[1]);
    public void PlayLevelThree() => asyncManager.LoadLevelButton(scenesLevels[2]);
}
