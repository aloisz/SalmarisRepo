using System;
using System.Collections;
using System.Collections.Generic;
using AI;
using Player;
using UnityEditor;
using UnityEngine;

public class AI_Smasher : AI_Pawn
{
    [Header("--- AI_Smasher ---")] 
    [SerializeField] protected SmasherMobState smasherMobState;
    
    [Header("--- Perimeter ---")] [SerializeField] [Range(0, 120)]
    private List<float> perimeters;
    
    protected enum SmasherMobState
    {
        Perimeter_0,
        Perimeter_1,
        Perimeter_2,
        Perimeter_3
    }
    
    protected SmasherMobState ChangeState(SmasherMobState state)
    {
        return this.smasherMobState = state;
    }
        
    protected override void PawnBehavior()
    {
        base.PawnBehavior();
        switch (smasherMobState)
        {
            case SmasherMobState.Perimeter_0:
                break;
            case SmasherMobState.Perimeter_1:
                break;
            case SmasherMobState.Perimeter_2:
                break;
            case SmasherMobState.Perimeter_3:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        CheckDistance();
    }
    
    /// <summary>
    /// Adapt the agent speed if the player is too far away
    /// </summary>
    /// <returns></returns>
    protected virtual void CheckDistance()
    {
        switch (Vector3.Distance(PlayerController.Instance.transform.position, transform.position))
        {
            case var value when value < perimeters[0]:
                ChangeState(SmasherMobState.Perimeter_0);
                break;
            case var value when value < perimeters[1]:
                ChangeState(SmasherMobState.Perimeter_1);
                break;
            case var value when value < perimeters[2]:
                ChangeState(SmasherMobState.Perimeter_2);
                break;
            case var value when value < perimeters[3]:
                ChangeState(SmasherMobState.Perimeter_3);
                break;
        }
    }
    
    // Debug -------------------------
    #if UNITY_EDITOR
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        var tr = transform;
        var pos = tr.position;
        
        DebugDistance(tr, pos);
        
        if(Application.isPlaying)
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.red;
            style.fontSize = 25;
            Handles.Label(pos + new Vector3(0,5,0), $"Sate {smasherMobState}", style);
        }
    }

    private void DebugDistance(Transform tr, Vector3 pos)
    {
        for (int i = 0; i < perimeters.Count; i++)
        {
            Color32 color = new Color32();
            switch (i)
            {
                case 0:
                    color = new Color32(0, 125, 255, 70); 
                    break;
                case 1:
                    color = new Color32(0, 125, 255, 50); 
                    break;
                case 2:
                    color = new Color32(0, 125, 255, 35); 
                    break;
                case 3:
                    color = new Color32(0, 125, 255, 20); 
                    break;
            }
            Handles.color = color;
            Handles.DrawSolidDisc(pos, tr.up, perimeters[i]);
        }
    }
    
    #endif
    
}
