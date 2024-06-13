using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class ShaderUnscaledTime : MonoBehaviour
{
    private Graphic _renderer;
    private Material _material;

    private void Start()
    {
        _renderer = GetComponent<Graphic>();
        _material = _renderer.material;
        
        var mat = Instantiate(_material);
        _renderer.material = mat;
    }

    // Update is called once per frame
    void Update()
    {
        _renderer.material.SetFloat("_UnscaledTime", Time.unscaledTime);
    }
}
