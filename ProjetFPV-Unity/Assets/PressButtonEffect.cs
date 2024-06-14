using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class PressButtonEffect : MonoBehaviour, IPointerClickHandler, IResetEffect
{
    [SerializeField] private float multiplier;
    [SerializeField] private float duration;
    [SerializeField] private AnimationCurve curve;
    private Vector3 _baseScale;
    
    // Start is called before the first frame update
    void Start()
    {
        _baseScale = transform.localScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        transform.DOScale(_baseScale * multiplier, duration).SetEase(curve).SetUpdate(true);
    }
    
    public void ResetEffect()
    {
        transform.localScale = _baseScale;
    }
}
