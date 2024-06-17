using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : GenericSingletonClass<LoadingScreen>
{
    [SerializeField] private Image loadingBar;
    [SerializeField] private Canvas loadingScreenContainer;
    [SerializeField] private TextMeshProUGUI loadingScreenText;
    
    public override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        loadingScreenContainer.enabled = false;
    }

    public void InitLoading()
    {
        loadingScreenContainer.enabled = true;
    }
    
    public void CloseLoading()
    {
        loadingScreenContainer.enabled = false;
    }

    public void UpdateFiller(float value)
    {
        loadingBar.fillAmount = value;
        loadingScreenText.text = value * 100 + "%";
    }
}
