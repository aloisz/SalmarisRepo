using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TitleScreen : MonoBehaviour
{
    [SerializeField] private GameObject optionMenu;
    [SerializeField] private GameObject levelSelectionMenu;
    
    [SerializeField] private Animator credits;

    public void OpenOptions() => optionMenu.SetActive(true);
    public void CloseOptions() => optionMenu.SetActive(false);
    public void Quit() => Application.Quit();

    public void OpenCredits() => credits.SetTrigger("Open");
    public void CloseCredits() => credits.SetTrigger("Close");
    
    public void OpenLevelSelection() => levelSelectionMenu.SetActive(true);
    public void CloseLevelSelection() => levelSelectionMenu.SetActive(false);

    public void PlayLevel(int index) => GameManager.Instance.ChangeLevel(index);
}
