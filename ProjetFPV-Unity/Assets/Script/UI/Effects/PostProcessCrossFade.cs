using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Script;
using UnityEngine;
using UnityEngine.Rendering;

public class PostProcessCrossFade : GenericSingletonClass<PostProcessCrossFade>, IDestroyInstance
{
    private List<Volume> volumes = new List<Volume>();

    public override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        volumes.Add(GameObject.FindWithTag("PP1").GetComponent<Volume>());
        volumes.Add(GameObject.FindWithTag("PP2").GetComponent<Volume>());
    }

    public void CrossFadeTo(int index)
    {
        volumes[index].DOVolumeWeight(1f, 0.7f).SetUpdate(true);
        volumes[index == 0 ? 1 : 0].DOVolumeWeight(0f, 0.7f).SetUpdate(true);
    }
    
    public void DestroyInstance()
    {
        Destroy(gameObject);
    }
}
