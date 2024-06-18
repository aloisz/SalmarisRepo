using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class TitleScreen : MonoBehaviour
{
    [SerializeField] private GameObject optionMenu;
    
    [SerializeField] private Animator credits;
    [SerializeField] private Animator levelSelect;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.75f);
        MusicManager.Instance.ChangeMusicPlayed(Music.Shop, 2f, 0.35f);
    }

    public void OpenOptions() => optionMenu.SetActive(true);
    public void CloseOptions() => optionMenu.SetActive(false);
    public void Quit() => Application.Quit();

    public void OpenCredits() => credits.SetTrigger("Open");
    public void CloseCredits() => credits.SetTrigger("Close");

    public void OpenLevelSelection() => levelSelect.SetTrigger("Open");
    public void CloseLevelSelection() => levelSelect.SetTrigger("Close");

    public void PlayLevel(int index)
    {
        LoadingScreen.Instance.InitLoading();
        GameManager.Instance.ChangeLevel(index); 
    }
}
