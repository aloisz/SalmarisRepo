using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

public class DecalParameters : MonoBehaviour
{
    [SerializeField] private bool logicOnStart;
    
    [SerializeField] private bool justUseAlphaFade;
    [ShowIf("justUseAlphaFade")] [SerializeField] private float alphaFadeDelay;
    
    [SerializeField] private PropertyDissolve[] property;

    private DecalProjector decal;

    private void Awake()
    {
        decal = GetComponent<DecalProjector>();
        CreateMaterialInstance(decal);

        if (logicOnStart) SpawnDecal();
    }

    private void CreateMaterialInstance(DecalProjector decalToGetMaterialFrom)
    {
        var material = Instantiate(decalToGetMaterialFrom.material);
        decalToGetMaterialFrom.material = material;
    }

    private void OnEnable()
    {
        decal.fadeFactor = 1f;
        
        foreach (PropertyDissolve p in property)
        {
            decal.material.SetFloat(p.propertyNameFade, 1f);
            decal.material.SetFloat(p.propertyName, 1f);
        }
    }

    public void SpawnDecal(string key = "")
    {
        if (justUseAlphaFade)
        {
            StartCoroutine(FadeDecal(alphaFadeDelay));
            Pooling.Instance.DelayedDePop(key, gameObject, alphaFadeDelay);
            return;
        }
        
        var lifeTime = 0f;
        
        foreach (PropertyDissolve p in property)
        {
            if (p.fadeIn)
            {
                decal.material.SetFloat(p.propertyNameFade, 0f);
                decal.material.DOFloat(p.finalValueFade, p.propertyNameFade, p.animDurationFade).SetDelay(p.delayBeforeFinalValueFade);
                lifeTime += p.animDurationFade + p.delayBeforeFinalValueFade;
            }
            
            var rand = Random.Range(0.9f, 1.1f);
            decal.material.DOFloat(p.finalValue, p.propertyName, p.animDuration * rand).SetDelay(p.delayBeforeFinalValue + p.delayBeforeFinalValueFade);
            lifeTime += p.animDuration * rand + p.delayBeforeFinalValue;
        }
        
        if(key != String.Empty) Pooling.Instance.DelayedDePop(key, gameObject, lifeTime);
        else Destroy(gameObject, lifeTime);
    }

    IEnumerator FadeDecal(float fadeDuration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0, elapsedTime / fadeDuration);

            decal.fadeFactor = alpha;

            yield return null;
        }
    }
}

[Serializable]
public class PropertyDissolve
{
    public string propertyName;
    public float animDuration;
    public float delayBeforeFinalValue;
    public float finalValue;
    
    [Space(10f)]
    
    public bool fadeIn;
    public string propertyNameFade;
    public float animDurationFade;
    public float delayBeforeFinalValueFade;
    public float finalValueFade;
}
