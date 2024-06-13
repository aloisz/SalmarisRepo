using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BreathEffect : MonoBehaviour
{
    [SerializeField] private Vector2 offset;
    [SerializeField] private float duration;
    [SerializeField] private AnimationCurve curve;
    private Vector2 _basePosition;
    
    // Start is called before the first frame update
    void Start()
    {
        _basePosition = GetComponent<RectTransform>().anchoredPosition;
        GetComponent<RectTransform>().DOAnchorPos(_basePosition + offset, duration).SetUpdate(true).SetLoops(-1)
            .SetEase(curve);
    }
}
