using System;
using System.Collections;
using System.Collections.Generic;
using AI;
using DG.Tweening;
using Player;
using UnityEngine;
using UnityEngine.UI;

public class Spot_Enemies : MonoBehaviour
{
    private Transform _spot;
    private AI_Pawn pawn;
    
    [SerializeField] private float maxDistance = 50f;
    [SerializeField] private float maxDistanceToSee = 4f;
    [SerializeField] private float maxScale = 0.5f;

    private void Start()
    {
        pawn = GetComponentInParent<AI_Pawn>();
        _spot = transform;
        
        pawn.onEnemyDead += () => _spot.transform.DOScale(Vector3.zero, 0.25f);
    }

    // Update is called once per frame
    void Update()
    {
        if (_spot is null || pawn.isPawnDead) return;

        var distanceWithPlayer = Vector3.Distance(PlayerController.Instance.transform.position, _spot.position);

        if (distanceWithPlayer >= maxDistanceToSee)
        {
            _spot.localScale = Vector3.zero;
            return;
        }
        
        var scaleUniform = new Vector3(maxScale, maxScale, maxScale);
        //_spot.localScale = Vector3.Lerp(scaleUniform, Vector3.zero, distanceWithPlayer / maxDistance);
        _spot.localScale = Vector3.Lerp(Vector3.zero, scaleUniform, (distanceWithPlayer - 12) / maxDistance);
    }
}
