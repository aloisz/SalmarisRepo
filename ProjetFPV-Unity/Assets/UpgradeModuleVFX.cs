using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Player;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeModuleVFX : GenericSingletonClass<UpgradeModuleVFX>
{
    [SerializeField] private ParticleSystem[] reactors;
    [SerializeField] private ParticleSystem[] trails;
    [SerializeField] private ParticleSystem landVFX;
    [SerializeField] private ParticleSystem balise;
    [SerializeField] private MeshRenderer screen;
    [SerializeField] private AnimationCurve screenCurve;

    private void Start()
    {
        CreateMaterialInstance(screen);
    }

    public void LandVFX()
    {
        RaycastHit hit;
        Physics.Raycast(transform.position, Vector3.down, out hit, 500f, PlayerController.Instance.groundLayer);
        var vfx = Instantiate(landVFX, hit.point - new Vector3(0,4f,0), Quaternion.identity);
        vfx.transform.rotation = Quaternion.Euler(new Vector3(-90,0,0));
        vfx.Play();
        
        screen.material.DOFloat(1f, "_Emissive", 0.45f).SetEase(screenCurve).SetDelay(1f);
    }

    public void StartLanding()
    {
        landVFX.Stop();
        balise.Play();
        foreach (var ps in reactors) ps.Stop();
        foreach (var ps in trails) ps.Stop();
        
        screen.material.SetFloat("_Emissive", 0f);
    }
    
    public void GoAwayVFX()
    {
        foreach (var ps in reactors) ps.Play();
        foreach (var ps in trails) ps.Play();
        balise.Stop();
        screen.material.DOFloat(0f, "_Emissive", 0.5f);
    }

    private void CreateMaterialInstance(MeshRenderer meshRenderer)
    {
        var mat = Instantiate(meshRenderer.material);
        meshRenderer.material = mat;
    }
}
