using System;
using System.Collections;
using System.Collections.Generic;
using Coffee.UIExtensions;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TitleButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float sizeMultiplier = 1.25f;
    [SerializeField] private float duration = 1.25f;
    [SerializeField] private int maxCharSpacing;
    [SerializeField] private float[] moveX;
    [SerializeField] private AnimationCurve curve;

    [SerializeField] private Color[] colors;
        
    [SerializeField] private Image markerImg;
    
    [SerializeField] private ParticleSystem particles;
    
    private Vector3 _baseSize;
    private TextMeshProUGUI text;
    private Vector3 _basePosition;
    private Vector3 _basePositionImg;

    private void Start()
    {
        _baseSize = transform.localScale;
        text = GetComponentInChildren<TextMeshProUGUI>();
        _basePosition = text.rectTransform.anchoredPosition;
        _basePositionImg = markerImg.rectTransform.anchoredPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        text.DOColor(colors[1], duration).SetEase(curve);
        text.DOCharacterSpacing(maxCharSpacing, duration).SetEase(curve);
        
        text.transform.DOScale(_baseSize * sizeMultiplier, duration).SetEase(curve);
        
        text.rectTransform.DOAnchorPosX(_basePosition.x + moveX[0], duration).SetEase(curve);
        markerImg.rectTransform.DOAnchorPosX(_basePositionImg.x + moveX[1], duration).SetEase(curve);
        
        markerImg.rectTransform.rotation = Quaternion.Euler(Vector3.zero);
        markerImg.rectTransform.DORotate(new Vector3(0,0,-90), duration).SetEase(curve);
        
        particles.Stop();
        particles.Play();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        text.DOColor(colors[0], duration).SetEase(curve);
        text.DOCharacterSpacing(0, duration).SetEase(curve);
        
        text.transform.DOScale(_baseSize, duration).SetEase(curve);
        
        text.rectTransform.DOAnchorPosX(_basePosition.x, duration).SetEase(curve);
        markerImg.rectTransform.DOAnchorPosX(_basePositionImg.x, duration).SetEase(curve);
        
        particles.Stop();
    }
}
