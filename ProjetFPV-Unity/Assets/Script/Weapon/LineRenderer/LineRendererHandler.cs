using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LineRendererHandler : MonoBehaviour
{
    public float time = 0.2f;

    private LineRenderer lineRenderer;
    
    private IEnumerator DePop()
    {
        lineRenderer = GetComponent<LineRenderer>();
        yield return new WaitForSeconds(time);
        Pooling.instance.DelayedDePop("HitScanRay", gameObject,0);
    }

    private void OnEnable()
    {
        StartCoroutine(DePop());
    }
}
