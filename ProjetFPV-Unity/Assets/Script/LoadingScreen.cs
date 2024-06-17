using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : GenericSingletonClass<LoadingScreen>
{
    [SerializeField] private Image loadingBar;
    [SerializeField] private Image loadingCircle;
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
        loadingCircle.transform.DORotate(new Vector3(0, 0, 360), 2f).SetLoops(-1);
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
