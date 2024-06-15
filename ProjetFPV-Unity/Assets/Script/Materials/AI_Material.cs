using System;
using System.Collections;
using System.Collections.Generic;
using AI;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class AI_Material : MonoBehaviour
{
    [SerializeField] private float saturationAmount = 0.75f;
    [SerializeField] private float lightningAmount = 0.75f;
    [SerializeField] public float deathDuration = 0.75f;
    [SerializeField] public float deathDissolveDuration = 0.75f;
    [SerializeField] public float dissolveDelay = 0.75f;

    private Color baseEyesColor;
    private SkinnedMeshRenderer _meshRenderer;
    
    // Start is called before the first frame update
    void Start()
    {
        _meshRenderer = GetComponent<SkinnedMeshRenderer>();
        if(_meshRenderer) CreateMaterialInstance(_meshRenderer);

        baseEyesColor = _meshRenderer.material.GetColor("_EmissiveColor");
    }

    public void Death()
    {
        if (!_meshRenderer) return;
        
        _meshRenderer.material.DOFloat(saturationAmount, "_DeathSaturation", deathDuration);
        _meshRenderer.material.DOFloat(lightningAmount, "_DeathDarkening", deathDuration);
        _meshRenderer.material.DOColor(Color.black, "_EmissiveColor", deathDuration / 3f);
        _meshRenderer.material.SetInt("_BellyVFX", 0);
        _meshRenderer.material.DOFloat(1f, "_DeathDissolve", deathDissolveDuration).SetDelay(dissolveDelay);
    }

    public void Reset()
    {
        _meshRenderer = GetComponent<SkinnedMeshRenderer>();
        if (!_meshRenderer) return;
        
        if(_meshRenderer) CreateMaterialInstance(_meshRenderer);
        
        _meshRenderer.material.SetFloat("_DeathSaturation", 1f);
        _meshRenderer.material.SetFloat("_DeathDarkening", 1f);
        _meshRenderer.material.SetFloat("_DeathDissolve", 0f);
        _meshRenderer.material.SetInt("_BellyVFX", 1);
        _meshRenderer.material.SetColor("_EmissiveColor", baseEyesColor);
    }

    private void CreateMaterialInstance(SkinnedMeshRenderer meshRenderer)
    {
        var mat = Instantiate(meshRenderer.material);
        meshRenderer.material = mat;
    }
}
