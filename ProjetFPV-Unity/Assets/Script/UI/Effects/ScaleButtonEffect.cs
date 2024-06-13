using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Transform))]
public class ScaleButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IResetEffect
{
    [SerializeField] private float multiplier;
    [SerializeField] private float duration;
    
    [SerializeField] private bool textScale;
    [ShowIf("textScale")][SerializeField] private float textScaleMultiplier;
    [ShowIf("textScale")][SerializeField] private float textDuration;
    
    private Vector3 _baseScale;
    private Vector3 _baseScaleText;
    
    // Start is called before the first frame update
    void Start()
    {
        _baseScale = transform.localScale;
        if (textScale) _baseScaleText = GetComponentInChildren<TextMeshProUGUI>().rectTransform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(_baseScale * multiplier, duration).SetUpdate(true);
        if (textScale) GetComponentInChildren<TextMeshProUGUI>().rectTransform.
            DOScale(_baseScaleText * textScaleMultiplier, textDuration).SetUpdate(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetEffect();
    }

    public void ResetEffect()
    {
        transform.DOScale(_baseScale, duration).SetUpdate(true);
        if (textScale) GetComponentInChildren<TextMeshProUGUI>().rectTransform.
            DOScale(_baseScaleText, textDuration).SetUpdate(true);
    }
}
