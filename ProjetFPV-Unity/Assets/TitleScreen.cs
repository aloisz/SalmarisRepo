using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScreen : MonoBehaviour
{
    [SerializeField] private GameObject optionMenu;
    [SerializeField] private AsyncManager asyncManager;

    public void OpenOptions() => optionMenu.SetActive(true);
    public void CloseOptions() => optionMenu.SetActive(false);

    public void Play() => asyncManager.LoadLevelButton();
}
