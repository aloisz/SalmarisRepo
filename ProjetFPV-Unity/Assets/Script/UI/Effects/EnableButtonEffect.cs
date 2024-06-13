using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class EnableButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IResetEffect
{
    [SerializeField] private GameObject[] objectsToManage;

    private void Start()
    {
        ResetEffect();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        foreach (var obj in objectsToManage)
        {
            obj.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetEffect();
    }

    public void ResetEffect()
    {
        foreach (var obj in objectsToManage)
        {
            obj.SetActive(false);
        }
    }
}
