using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using NaughtyAttributes;
using Random = UnityEngine.Random;

public class LineRendererHandler : MonoBehaviour
{
    [MinMaxSlider(0, 5)]public Vector2 time ;

    private LineRenderer lineRenderer;
    
    private IEnumerator DePop()
    {
        lineRenderer = GetComponent<LineRenderer>();
        yield return new WaitForSeconds(Random.Range(time.x, time.y));
        Pooling.instance.DelayedDePop("HitScanRay", gameObject,0);
    }

    private void OnEnable()
    {
        StartCoroutine(DePop());
    }
}
