using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;

public class UpgradeModule : GenericSingletonClass<UpgradeModule>
{
    [SerializeField][CurveRange(0,0,1,1, EColor.Orange)] private AnimationCurve landingCurve;
    [SerializeField] private float landingDuration;

    [SerializeField][CurveRange(0,0,1,1, EColor.Orange)] private AnimationCurve leaveCurve;
    [SerializeField] private float leaveDuration;
    
    [SerializeField] private float fullRotateAmount;
    
    [SerializeField] private float offsetLandingY;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float anticipationRaycastLenght;
    
    private RaycastHit _hitGroundLanding;
    private Vector3 orbitPosition;
    private Vector3 _baseScale;

    private void Start()
    {
        _baseScale = transform.localScale;
        transform.localScale = Vector3.zero;
    }

    public void InitModule(Vector3 position)
    {
        orbitPosition = position;

        var t = transform;
        
        t.position = orbitPosition;
        t.localScale = Vector3.zero;
        
        CheckGroundLandingPosition();
        
        if (_hitGroundLanding.collider is null) 
            throw new Exception("Cannot land the module because it can't found the ground.");
        
        t.DOMove(_hitGroundLanding.point + new Vector3(0, offsetLandingY, 0), 
            landingDuration).SetEase(landingCurve).OnComplete(LeftModule);
        t.DOScale(_baseScale, landingDuration).SetEase(landingCurve);
        t.DORotate(new Vector3(0, 360f * fullRotateAmount, 0), landingDuration, 
            RotateMode.FastBeyond360).SetEase(landingCurve);
    }

    public void LeftModule()
    {
        var t = transform;
        
        t.DOMove(orbitPosition, leaveDuration).SetEase(leaveCurve);
        t.DOScale(Vector3.zero, leaveDuration).SetEase(leaveCurve);
        t.DORotate(new Vector3(0, -360f * fullRotateAmount, 0), leaveDuration, 
            RotateMode.FastBeyond360).SetEase(leaveCurve);
    }

    private void CheckGroundLandingPosition()
    {
        Physics.Raycast(transform.position, Vector3.down, 
            out _hitGroundLanding, anticipationRaycastLenght, groundMask);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0.5f, 0);
        Gizmos.DrawRay(transform.position, Vector3.down * anticipationRaycastLenght);
        if(_hitGroundLanding.collider) Gizmos.DrawSphere(_hitGroundLanding.point, 0.5f);
    }
}
