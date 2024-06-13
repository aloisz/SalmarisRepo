using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Outline))]
public class OutlineButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IResetEffect
{
    [SerializeField] private float duration;
    
    private Outline _outline;
    // Start is called before the first frame update
    void Start()
    {
        _outline = GetComponent<Outline>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _outline.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetEffect();
    }

    public void ResetEffect()
    {
        _outline.enabled = false;
    }
}
