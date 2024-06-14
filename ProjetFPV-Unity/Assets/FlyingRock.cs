using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class FlyingRock : MonoBehaviour
{
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private float floatingDuration;
    [SerializeField] private float offsetY;

    private Vector3 _basePosition;
    
    // Start is called before the first frame update
    void Start()
    {
        _basePosition = transform.position;
        transform.DOMoveY(_basePosition.y + offsetY, floatingDuration).SetEase(curve).SetLoops(-1);
    }
}
