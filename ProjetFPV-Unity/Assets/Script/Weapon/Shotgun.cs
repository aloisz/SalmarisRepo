using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Weapon;

public class Shotgun : ShootingLogicModule
{
    private bool isRocketJumping;
    private Vector3 hitPoint;
    
    protected override void RaycastEnum()
    {
        base.RaycastEnum();
        if (!so_Weapon.weaponMode[(int)actualWeaponModeIndex].isRocketJump) return;
        RaycastSingleHitScanRocketJump(so_Weapon.weaponMode[(int)actualWeaponModeIndex].rocketJumpDistance);
    }
    
    public void RaycastSingleHitScanRocketJump(float maxDistance)
    {
        RaycastHit hit;
        if (Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, maxDistance, so_Weapon.hitLayer))
        {
            Debug.DrawRay(camera.transform.position, camera.transform.forward * maxDistance, Color.red, .2f);
            
            if (!so_Weapon.weaponMode[(int)actualWeaponModeIndex].isRocketJump) return;
            if (hit.transform.GetComponent<Collider>() != null)
            {
                this.hitPoint = hit.point; 
                isRocketJumping = true;
            }
        }
    }

    private void FixedUpdate()
    {
        if(!isRocketJumping) return;
        PlayerController.GetComponent<Rigidbody>().AddForce( (PlayerController.transform.position - hitPoint).normalized * so_Weapon.weaponMode[(int)actualWeaponModeIndex].rocketJumpForceApplied);
        isRocketJumping = false;
    }
}
