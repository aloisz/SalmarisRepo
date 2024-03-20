using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VladInput : MonoBehaviour
{
    public bool isReceivingPrimary;
    public bool isReceivingSecondary;
    public bool isReceivingReload;
    
    public void Primary(InputAction.CallbackContext ctx)
    {
        isReceivingPrimary = ctx.performed;
    }
    
    public void Secondary(InputAction.CallbackContext ctx)
    {
        isReceivingSecondary = ctx.performed;
    }
    
    public void Reload(InputAction.CallbackContext ctx)
    {
        isReceivingReload = ctx.performed;
    }
}
