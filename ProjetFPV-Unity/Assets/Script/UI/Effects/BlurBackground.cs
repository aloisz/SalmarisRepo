using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class BlurBackground : MonoBehaviour
{
    [SerializeField] private Volume[] volumes;
    
    public static BlurBackground instance;

    private void Awake()
    {
        instance = this;
    }
    
    public void Blur(bool enabled, float transitionDuration)
    {
        volumes[0].DOVolumeWeight(enabled ? 0f : 1f, transitionDuration).SetUpdate(true);
        volumes[1].DOVolumeWeight(enabled ? 1f : 0f, transitionDuration).SetUpdate(true);
    }
}
