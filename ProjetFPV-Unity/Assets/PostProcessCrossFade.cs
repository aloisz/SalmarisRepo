using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

public class PostProcessCrossFade : GenericSingletonClass<PostProcessCrossFade>
{
    [SerializeField] private Volume[] volumes;

    public void CrossFadeTo(int index)
    {
        volumes[index].DOVolumeWeight(1f, 0.7f).SetUpdate(true);
        volumes[index == 0 ? 1 : 0].DOVolumeWeight(0f, 0.7f).SetUpdate(true);
    }
}
