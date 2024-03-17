using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Weapon;
using Weapon.Interface;

public class TestModule : MonoBehaviour, IShootRaycast, IShootSphereCast
{
    private WeaponManager weaponManager;

    private void Start()
    {
        weaponManager = GetComponent<WeaponManager>();
    }
    /*
    private void ChooseEnum()
    {
        switch (ChooseRaycastType(weaponManager.so_Weapon.weaponMode[(int)weaponManager.actualWeaponModeIndex].raycastType))
        {
            case RaycastType.Raycast:
                if (so_Weapon.weaponMode[(int)actualWeaponModeIndex].isHavingDispersion) RaycastDispersionHitScan(GetTheDistance());
                else RaycastSingleHitScan(GetTheDistance());
                break;
            
            case RaycastType.SphereCast:
                float radius = so_Weapon.weaponMode[(int)actualWeaponModeIndex].sphereCastRadius;
                if (so_Weapon.weaponMode[(int)actualWeaponModeIndex].isHavingDispersion) SphereCastDispersionHitScan(GetTheDistance(), radius) ;
                else SphereCastSingleHitScan(GetTheDistance(), radius);
                break;
        }
    }
    */

    public void RaycastSingleHitScan(float maxDistance)
    {
        throw new System.NotImplementedException();
    }

    public void RaycastDispersionHitScan(float maxDistance)
    {
        throw new System.NotImplementedException();
    }

    public void SphereCastSingleHitScan(float maxDistance, float radius)
    {
        throw new System.NotImplementedException();
    }

    public void SphereCastDispersionHitScan(float maxDistance, float radius)
    {
        throw new System.NotImplementedException();
    }

    public RaycastType ChooseRaycastType(RaycastType raycastType)
    {
        throw new NotImplementedException();
    }
}
