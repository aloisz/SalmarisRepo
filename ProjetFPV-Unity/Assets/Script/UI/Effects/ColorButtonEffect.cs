using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ColorButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IResetEffect
{
    [SerializeField] private Color finalColor;
    [SerializeField] private float duration;
    
    private Color _baseColor;
    private Image _image;
    
    // Start is called before the first frame update
    void Start()
    {
        _image = GetComponent<Image>();
        _baseColor = _image.color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _image.DOColor(finalColor, duration).SetUpdate(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetEffect();
    }

    public void ResetEffect()
    {
        _image.DOColor(_baseColor, duration).SetUpdate(true);
    }
}
