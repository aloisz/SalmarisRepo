using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Weapon;

public class PlayerInputs : GenericSingletonClass<PlayerInputs>
{
    public bool isReceivingMoveInputs;
    public bool isReceivingDashInputs;
    public bool isReceivingJumpInputs;
    public bool isReceivingSlideInputs;

    public Action onJump;

    public Vector2 moveValue;
    public Vector2 rotateValue;
    
    //WeaponSwap
    public List<WeaponManager> weapons;
    public bool isOnMainWeapon = true;

    private void Start()
    {
        GetWeaponByIndex(GetIndexByBoolean(isOnMainWeapon));
    }


    /// <summary>
    /// Get the moving inputs.
    /// </summary>
    /// <param name="ctx">Automatic parameter to get the current input values.</param>
    public void MoveInputs(InputAction.CallbackContext ctx)
    {
        isReceivingMoveInputs = ctx.performed;
        moveValue = ctx.ReadValue<Vector2>();
    }
    
    /// <summary>
    /// Get the dash input.
    /// </summary>
    /// <param name="ctx">Automatic parameter to get the current input values.</param>
    public void DashInput(InputAction.CallbackContext ctx)
    {
        isReceivingDashInputs = ctx.performed;
    }
    
    /// <summary>
    /// Get the jump input.
    /// </summary>
    /// <param name="ctx">Automatic parameter to get the current input values.</param>
    public void JumpInput(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            onJump += PlayerController.Instance.VerifyJumpExecution;
        }
    }
    
    /// <summary>
    /// Get the slide input.
    /// </summary>
    /// <param name="ctx">Automatic parameter to get the current input values.</param>
    public void SlideInput(InputAction.CallbackContext ctx)
    {
        isReceivingSlideInputs = ctx.performed;
    }

    /// <summary>
    /// Get the look input.
    /// </summary>
    public void LookInputs(InputAction.CallbackContext ctx)
    {
        rotateValue = ctx.ReadValue<Vector2>();
    }

    /// <summary>
    /// SwapWeapon
    /// </summary>
    public void SwapWeapon(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            isOnMainWeapon = !isOnMainWeapon;
            GetWeaponByIndex(GetIndexByBoolean(isOnMainWeapon));
            
            foreach (var weapon in weapons)
            {
                weapon.SwapWeapon();
            }
        }
    }

    void GetWeaponByIndex(int index)
    {
        weapons[index].transform.gameObject.SetActive(true);
        weapons[(index == 0 ? 1 : 0)].transform.gameObject.SetActive(false);
    }
    
    public int GetIndexByBoolean(bool b) => Convert.ToInt32(b);
}
