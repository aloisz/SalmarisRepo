using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BarbatosInput : MonoBehaviour
{
    private Barbatos barbatos;

    public bool isReceivingPrimary;
    public bool isReceivingSecondary;
    public bool isReceivingReload;

    private void Start()
    {
        barbatos = GetComponent<Barbatos>();
    }
    
    public void Primary(InputAction.CallbackContext ctx)
    {
        isReceivingPrimary = ctx.performed;
    }
    
    public void Secondary(InputAction.CallbackContext ctx)
    {
        //isReceivingSecondary = ctx.performed;
        if (ctx.started)
        {
            barbatos.Secondary();
        }
    }
    
    public void Reload(InputAction.CallbackContext ctx)
    {
        isReceivingReload = ctx.performed;
    }
}
