using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class TitleButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float sizeMultiplier = 1.25f;
    [SerializeField] private float sizeDuration = 1.25f;
    [SerializeField] private AnimationCurve sizeCurve;
    
    private Vector3 _baseSize;

    private void Start()
    {
        _baseSize = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(_baseSize * sizeMultiplier, sizeDuration).SetEase(sizeCurve);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(_baseSize, sizeDuration).SetEase(sizeCurve);
    }
}
