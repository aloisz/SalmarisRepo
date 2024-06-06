using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;

public class PlayerVFX : GenericSingletonClass<PlayerVFX>
{
    [Header("Slide")]
    [SerializeField] private ParticleSystem smokeSlide;
    
    [Header("Jump")]
    [SerializeField] private ParticleSystem landSmoke;

    public void SpawnLandSmoke()
    {
        landSmoke.Play();
    }

    private void Update()
    {
        ManageSmokeSlide();
    }

    private void ManageSmokeSlide()
    {
        if(!smokeSlide.isPlaying && PlayerController.Instance.isSliding) smokeSlide.Play();
        if(smokeSlide.isPlaying && !PlayerController.Instance.isSliding) smokeSlide.Stop();
    }
}
