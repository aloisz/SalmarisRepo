using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Weapon;
using Weapon.Interface;
using Random = UnityEngine.Random;

public class RaycastModule : MonoBehaviour, IShootRaycast, IShootSphereCast
{
    [SerializeField] private Transform gunBarrelPos;
    private WeaponManager weaponManager;

    private void Start()
    {
        weaponManager = GetComponent<WeaponManager>();
    }
    
    public void ChooseEnum(RaycastType raycastType)
    {
        switch (raycastType)
        {
            case RaycastType.Raycast:
                if (weaponManager.so_Weapon.weaponMode[(int)weaponManager.actualWeaponModeIndex].isHavingDispersion) RaycastDispersionHitScan(GetTheDistance());
                else RaycastSingleHitScan(GetTheDistance());
                break;
            
            case RaycastType.SphereCast:
                float radius = weaponManager.so_Weapon.weaponMode[(int)weaponManager.actualWeaponModeIndex].sphereCastRadius;
                if (weaponManager.so_Weapon.weaponMode[(int)weaponManager.actualWeaponModeIndex].isHavingDispersion) SphereCastDispersionHitScan(GetTheDistance(), radius) ;
                else SphereCastSingleHitScan(GetTheDistance(), radius);
                break;
        }
    }
    
    
    protected virtual void HitScanLogic(RaycastHit hit)
    {
        if (hit.transform.GetComponent<Collider>() != null)
        {
            weaponManager.InstantiateBulletImpact(hit);
        }
        if (hit.transform.GetComponent<IDamage>() != null)
        {
            hit.transform.GetComponent<IDamage>().Hit(weaponManager.so_Weapon.weaponMode[(int)weaponManager.actualWeaponModeIndex].bulletDamage);
        }
    }

    protected virtual void InitialiseLineRenderer(RaycastHit hit)
    {
        
        /*LineRenderer lineRenderer = Instantiate(GameManager.Instance.rayLineRenderer,
            weaponManager.camera.transform.position, Quaternion.identity, GameManager.Instance.transform);
        
        lineRenderer.startWidth = 
            weaponManager.so_Weapon.weaponMode[(int)weaponManager.actualWeaponModeIndex].raycastType == RaycastType.SphereCast ? 
                weaponManager.so_Weapon.weaponMode[(int)weaponManager.actualWeaponModeIndex].sphereCastRadius : 
                0.05f;
        
        lineRenderer.SetPosition(0, gunBarrelPos.position);
        lineRenderer.SetPosition(1, hit.point);*/
    }
    
    
    /// <summary>
    /// Return a random direction within the given parameters (zAxisDispersion, yAxisDispersion)
    /// </summary>
    /// <returns></returns>
    protected Vector3 GetTheDispersionDirection()
    {
        float zAxisDispersion = Random.Range(weaponManager.so_Weapon.weaponMode[(int)weaponManager.actualWeaponModeIndex].zAxisDispersion.x / 2f,
            weaponManager.so_Weapon.weaponMode[(int)weaponManager.actualWeaponModeIndex].zAxisDispersion.y / 2f);
                
        float yAxisDispersion = Random.Range(weaponManager.so_Weapon.weaponMode[(int)weaponManager.actualWeaponModeIndex].yAxisDispersion.x / 2f,
            weaponManager.so_Weapon.weaponMode[(int)weaponManager.actualWeaponModeIndex].yAxisDispersion.y / 2f);
                
        Vector3 direction = Quaternion.Euler(yAxisDispersion, zAxisDispersion , yAxisDispersion) * Vector3.forward;
        direction = weaponManager.camera.transform.rotation * direction;
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
            weaponManager.so_Weapon.weaponMode[(int)weaponManager.actualWeaponModeIndex].isRayDistanceNotInfinte 
                ? weaponManager.so_Weapon.weaponMode[(int)weaponManager.actualWeaponModeIndex].RayDistance 
                : 10000;
        return maxDistance;
    }
    

    public void RaycastSingleHitScan(float maxDistance)
    {
        RaycastHit hit;
        if (Physics.Raycast(weaponManager.camera.transform.position, weaponManager.camera.transform.forward, out hit, maxDistance, weaponManager.so_Weapon.hitLayer))
        {
            Debug.DrawRay(weaponManager.camera.transform.position, weaponManager.camera.transform.forward * maxDistance, Color.red, .2f);
            InitialiseLineRenderer(hit);
            
            HitScanLogic(hit);
        };
    }

    public void RaycastDispersionHitScan(float maxDistance)
    {
        RaycastHit hit;
        int howManyRay = Random.Range(weaponManager.so_Weapon.weaponMode[(int)weaponManager.actualWeaponModeIndex].howManyBulletShot.x,
            weaponManager.so_Weapon.weaponMode[(int)weaponManager.actualWeaponModeIndex].howManyBulletShot.y);
        for (int i = 0; i < howManyRay; i++)
        {
            if (Physics.Raycast(weaponManager.camera.transform.position,  GetTheDispersionDirection(), out hit, maxDistance, weaponManager.so_Weapon.hitLayer))
            {
                InitialiseLineRenderer(hit);
                Debug.DrawRay(weaponManager.camera.transform.position, GetTheDispersionDirection() * maxDistance, Color.red, .2f);
                HitScanLogic(hit);
            }
        }
    }

    public void SphereCastSingleHitScan(float maxDistance, float radius)
    {
        RaycastHit hit;
        if (Physics.SphereCast(weaponManager.camera.transform.position, radius, weaponManager.camera.transform.forward, out hit, maxDistance, weaponManager.so_Weapon.hitLayer))
        {
            Debug.DrawRay(weaponManager.camera.transform.position, weaponManager.camera.transform.forward * maxDistance, Color.red, .2f);
            InitialiseLineRenderer(hit);
            HitScanLogic(hit);
        }
    }

    public void SphereCastDispersionHitScan(float maxDistance, float radius)
    {
        RaycastHit hit;
        int howManyRay = Random.Range(weaponManager.so_Weapon.weaponMode[(int)weaponManager.actualWeaponModeIndex].howManyBulletShot.x,
            weaponManager.so_Weapon.weaponMode[(int)weaponManager.actualWeaponModeIndex].howManyBulletShot.y);
        for (int i = 0; i < howManyRay; i++)
        {
            if (Physics.SphereCast(weaponManager.camera.transform.position, radius,  GetTheDispersionDirection(), out hit, maxDistance, weaponManager.so_Weapon.hitLayer))
            {
                InitialiseLineRenderer(hit);
                Debug.DrawRay(weaponManager.camera.transform.position, GetTheDispersionDirection() * maxDistance, Color.red, .2f);
                HitScanLogic(hit);
            }
        }
    }

    public RaycastType ChooseRaycastType(RaycastType raycastType)
    {
        throw new NotImplementedException();
    }
}
