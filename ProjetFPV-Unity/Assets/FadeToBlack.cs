using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FadeToBlack : GenericSingletonClass<FadeToBlack>
{
    [SerializeField] private float duration;
    [SerializeField] private AnimationCurve curve;

    private Image image;
    private Canvas canvas;
    
    public override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        image = GetComponentInChildren<Image>();
        canvas = GetComponentInChildren<Canvas>();
        canvas.enabled = false;
    }

    public void Fade(bool loadScene = false, int index = 0, float delay = 0f)
    {
        StopAllCoroutines();
        StartCoroutine(FadeRoutine(loadScene, index, delay));
    }

    IEnumerator FadeRoutine(bool loadScene, int index, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        
        canvas.enabled = true;
        image.DOFade(1f, duration).SetEase(curve).OnComplete(()=> canvas.enabled = false);
        
        yield return new WaitForSecondsRealtime(3f);

        if (loadScene)
        {
            GameManager.Instance.ChangeLevel(index);
        }
    }
}
