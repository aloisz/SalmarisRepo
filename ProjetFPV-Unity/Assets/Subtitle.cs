using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class Subtitle : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private AnimationCurve curve;

    private void Start()
    {
        GetComponent<CanvasGroup>().alpha = 0f;
        GetComponent<CanvasGroup>().DOFade(1f, 0.2f).SetEase(curve).SetUpdate(true);
    }

    public void DestroySubtitle(float delay)
    {
        GetComponent<CanvasGroup>().DOFade(0f, 0.2f).SetEase(curve).SetDelay(delay).SetUpdate(true).OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }

    public void SetText(string text)
    {
        this.text.text = VoicelineManager.Instance.HighlightCustomTags(text);
    }
}
