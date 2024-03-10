using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Weapon;
using Weapon.Interface;

public class RaycastModule : WeaponManager, IShootRaycast, IShootSphereCast
{
    protected override void Raycast()
    {
        base.Raycast();
        ChooseEnum();
    }
    
    /// <summary>
    /// Where all the hitScan logic is applied
    /// </summary>
    /// <param name="hit"></param>
    protected virtual void HitScanLogic(RaycastHit hit)
    {
        if (hit.transform.GetComponent<Collider>() != null)
        {
            InstantiateBulletImpact(hit);
        }
        if (hit.transform.GetComponent<IDamage>() != null)
        {
            hit.transform.GetComponent<IDamage>().Hit(so_Weapon.weaponMode[(int)actualWeaponModeIndex].bulletDamage);
        }
    }

    protected virtual void InitialiseLineRenderer(RaycastHit hit)
    {
        LineRenderer lineRenderer = Instantiate(GameManager.Instance.rayLineRenderer,
            camera.transform.position, Quaternion.identity, GameManager.Instance.transform);
        lineRenderer.SetPosition(0, camera.transform.position);
        lineRenderer.SetPosition(1, hit.point);
    }
    
    /// <summary>
    /// Return a random direction within the given parameters (zAxisDispersion, yAxisDispersion)
    /// </summary>
    /// <returns></returns>
    protected Vector3 GetTheDispersionDirection()
    {
        float zAxisDispersion = Random.Range(so_Weapon.weaponMode[(int)actualWeaponModeIndex].zAxisDispersion.x / 2f,
            so_Weapon.weaponMode[(int)actualWeaponModeIndex].zAxisDispersion.y / 2f);
                
        float yAxisDispersion = Random.Range(so_Weapon.weaponMode[(int)actualWeaponModeIndex].yAxisDispersion.x / 2f,
            so_Weapon.weaponMode[(int)actualWeaponModeIndex].yAxisDispersion.y / 2f);
                
        Vector3 direction = Quaternion.Euler(yAxisDispersion, zAxisDispersion , yAxisDispersion) * Vector3.forward;
        direction = camera.transform.rotation * direction;
        return direction;
    }
        
    /// <summary>
    /// Return the distance for shooting Raycast
    /// </summary>
    /// <returns></returns>
    protected float GetTheDistance()
    {
        float maxDistance;
        maxDistance = 
            so_Weapon.weaponMode[(int)actualWeaponModeIndex].isRayDistanceNotInfinte 
                ? so_Weapon.weaponMode[(int)actualWeaponModeIndex].RayDistance 
                : 10000;
        return maxDistance;
    }
    

    private void ChooseEnum()
    {
        switch (ChooseRaycastType(so_Weapon.weaponMode[(int)actualWeaponModeIndex].raycastType))
        {
            case RaycastType.Raycast:
                if (so_Weapon.weaponMode[(int)actualWeaponModeIndex].isHavingDispersion) RaycastDispersionHitScan(GetTheDistance());
                else RaycastSingleHitScan(GetTheDistance());
                break;
            
            case RaycastType.SphereCast:
                float radius = so_Weapon.weaponMode[(int)actualWeaponModeIndex].sphereCastRadius;
                if (so_Weapon.weaponMode[(int)actualWeaponModeIndex].isHavingDispersion) SphereCastDispersionHitScan(GetTheDistance(), radius) ;
                else SphereCastSingleHitScan(GetTheDistance(),radius);
                break;
        }
    }

    #region Raycast

    public void RaycastSingleHitScan(float maxDistance)
    {
        RaycastHit hit;
        if (Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, maxDistance, so_Weapon.hitLayer))
        {
            Debug.DrawRay(camera.transform.position, camera.transform.forward * maxDistance, Color.red, .2f);
            InitialiseLineRenderer(hit);
            
            HitScanLogic(hit);
        }
    }

    public void RaycastDispersionHitScan(float maxDistance)
    {
        RaycastHit hit;
        int howManyRay = Random.Range(so_Weapon.weaponMode[(int)actualWeaponModeIndex].howManyBulletShot.x,
            so_Weapon.weaponMode[(int)actualWeaponModeIndex].howManyBulletShot.y);
        for (int i = 0; i < howManyRay; i++)
        {
            if (Physics.Raycast(camera.transform.position,  GetTheDispersionDirection(), out hit, maxDistance, so_Weapon.hitLayer))
            {
                InitialiseLineRenderer(hit);
                Debug.DrawRay(camera.transform.position, GetTheDispersionDirection() * maxDistance, Color.red, .2f);
                HitScanLogic(hit);
            }
        }
    }

    #endregion
    
    #region SphereCast

    public void SphereCastSingleHitScan(float maxDistance, float radius)
    {
        RaycastHit hit;
        if (Physics.SphereCast(camera.transform.position, radius, camera.transform.forward, out hit, maxDistance, so_Weapon.hitLayer))
        {
            Debug.DrawRay(camera.transform.position, camera.transform.forward * maxDistance, Color.red, .2f);
            InitialiseLineRenderer(hit);
            HitScanLogic(hit);
        }
    }

    public void SphereCastDispersionHitScan(float maxDistance, float radius)
    {
        RaycastHit hit;
        int howManyRay = Random.Range(so_Weapon.weaponMode[(int)actualWeaponModeIndex].howManyBulletShot.x,
            so_Weapon.weaponMode[(int)actualWeaponModeIndex].howManyBulletShot.y);
        for (int i = 0; i < howManyRay; i++)
        {
            if (Physics.SphereCast(camera.transform.position, radius,  GetTheDispersionDirection(), out hit, maxDistance, so_Weapon.hitLayer))
            {
                InitialiseLineRenderer(hit);
                Debug.DrawRay(camera.transform.position, GetTheDispersionDirection() * maxDistance, Color.red, .2f);
                HitScanLogic(hit);
            }
        }
    }

    #endregion

    
}
