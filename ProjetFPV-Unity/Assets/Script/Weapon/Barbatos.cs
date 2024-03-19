using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Weapon;
using Object = UnityEngine.Object;

public class Barbatos : Shotgun
{
    protected override void GetAllInput()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (actualWeaponModeIndex != WeaponMode.Primary)
            {
                actualWeaponModeIndex = WeaponMode.Primary;
                //WeaponRefreshement();
            }
            Shoot();
        }
        else
        {
            isShooting = false;
            canFire = true;
        }
            
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            if (actualWeaponModeIndex != WeaponMode.Secondary)
            {
                actualWeaponModeIndex = WeaponMode.Secondary;
                //WeaponRefreshement();
            }
            Shoot();
        }
        else
        {
            isShooting = false;
            canFire = true;
        }
            
        if (Input.GetKeyDown(KeyCode.R)) Reload();
    }


    protected override void HitScanLogic(RaycastHit hit)
    {
        base.HitScanLogic(hit);
        
        if (!so_Weapon.weaponMode[(int)actualWeaponModeIndex].isRocketJump) return;
        if (hit.transform.GetComponent<Collider>() != null)
        {
            PlayerController.GetComponent<Rigidbody>().AddForce( PlayerController.transform.position - hit.point * so_Weapon.weaponMode[(int)actualWeaponModeIndex].rocketForceApplied);
        }
    }
    
    public override void InstantiateBulletImpact(RaycastHit hit)
    {
        base.InstantiateBulletImpact(hit);
        /*GameObject particle =  Instantiate(GameManager.Instance.PS_BulletImpact, hit.point, Quaternion.identity, GameManager.Instance.transform);
        particle.transform.up = hit.normal;*/
        
        GameObject particle = Pooling.instance.Pop("BulletImpact");
        Pooling.instance.DelayedDePop("BulletImpact", particle,3);
        

        if (so_Weapon.weaponMode[(int)actualWeaponModeIndex].doExplosion)
        {
            GameObject explosion =  Instantiate(GameManager.Instance.explosion, hit.point, Quaternion.identity, GameManager.Instance.transform);
        }
    }
    
    
}
