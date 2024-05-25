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
    [SerializeField] private PropertyDissolve[] property;

    private DecalProjector decal;
    
    // Start is called before the first frame update
    void Start()
    {
        decal = GetComponent<DecalProjector>();
        
        CreateMaterialInstance(decal);

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

        Destroy(gameObject, lifeTime);
    }
    
    private void CreateMaterialInstance(DecalProjector decalToGetMaterialFrom)
    {
        var mat = Instantiate(decalToGetMaterialFrom.material);
        decalToGetMaterialFrom.material = mat;
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
