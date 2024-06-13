using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class MoveOnEnable : MonoBehaviour, IResetEffect
{
    [SerializeField] private Vector2 offset;
    [SerializeField] private bool basePosAsFinalPos;
    [SerializeField] private bool fadeIn;
    [ShowIf("fadeIn")][SerializeField] private float finalAlpha;
    [SerializeField] private float duration;

    private Vector2 _basePosition;
    private bool startInitiated;

    private void Start()
    {
        var rectTransform = GetComponent<RectTransform>();
        _basePosition = rectTransform.anchoredPosition;
        gameObject.SetActive(false);

        startInitiated = true;
    }

    private void OnEnable()
    {
        if (!startInitiated) return;
        
        var rectTransform = GetComponent<RectTransform>();
        
        if (basePosAsFinalPos)
        {
            rectTransform.anchoredPosition = _basePosition + offset;
            rectTransform.DOAnchorPos(_basePosition, duration).SetUpdate(true);
        }
        else
        {
            rectTransform.anchoredPosition = _basePosition;
            rectTransform.DOAnchorPos(_basePosition + offset, duration).SetUpdate(true);
        }
        
        if (fadeIn)
        {
            if (GetComponent<Image>())
            {
                var img = GetComponent<Image>();
                img.DOFade(0f, 0.01f);
                img.DOFade(finalAlpha, duration).SetUpdate(true);
            }
        }
    }

    public void ResetEffect()
    {
        var rectTransform = GetComponent<RectTransform>();
        rectTransform.DOAnchorPos(_basePosition, duration).SetUpdate(true);
        
        if (fadeIn)
        {
            if (GetComponent<Image>())
            {
                var img = GetComponent<Image>();
                img.DOFade(0f, duration).SetUpdate(true);
            }
        }
    }
}
