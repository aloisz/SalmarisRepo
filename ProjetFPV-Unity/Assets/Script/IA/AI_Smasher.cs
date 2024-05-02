using System;
using System.Collections;
using System.Collections.Generic;
using AI;
using Player;
using UnityEditor;
using UnityEngine;

public class AI_Smasher : AI_Pawn
{
    [Header("----- AI_Smasher -----")] 
    [SerializeField] protected SmasherMobState smasherMobState;
    
    [Header("--- Perimeter ---")] 
    public List<Perimeters> perimeters;

    [Header("Component")] 
    [SerializeField] protected AI_Smasher_Perimeter0 _aiSmasherPerimeter0;
    [SerializeField] protected AI_Smasher_Perimeter1 _aiSmasherPerimeter1;

    [Header("Debugging")] 
    [SerializeField] private bool enableDebugging;
    
    
    public enum SmasherMobState
    {
        Perimeter_0,
        Perimeter_1,
        Perimeter_2,
        Perimeter_3
    }
    
    public SmasherMobState ChangeState(SmasherMobState state)
    {
        return this.smasherMobState = state;
    }
        
    protected override void PawnBehavior()
    {
        base.PawnBehavior();
        switch (smasherMobState)
        {
            case SmasherMobState.Perimeter_0:
                _aiSmasherPerimeter0.HandlePerimeter0();
                break;
            case SmasherMobState.Perimeter_1:
                _aiSmasherPerimeter1.HandlePerimeter1();
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

    private bool canHit = false;
    private float strenght;
    private Vector3 direction;
    public void ApplyKnockBack(float strenght, Vector3 dir)
    {
        canHit = true;
        this.strenght = strenght;
        this.direction = dir;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        
        if (!canHit) return;
        canHit = false;
        PlayerController.Instance._rb.AddForce(direction * strenght);
    }
    
    /// <summary>
    /// Adapt the agent speed if the player is too far away
    /// </summary>
    /// <returns></returns>
    protected virtual void CheckDistance()
    {
        switch (Vector3.Distance(PlayerController.Instance.transform.position, transform.position))
        {
            case var value when value < perimeters[0].distToEnemy:
                ChangeState(SmasherMobState.Perimeter_0);
                break;
            case var value when value < perimeters[1].distToEnemy:
                ChangeState(SmasherMobState.Perimeter_1);
                break;
            case var value when value < perimeters[2].distToEnemy:
                ChangeState(SmasherMobState.Perimeter_2);
                break;
            case var value when value < perimeters[3].distToEnemy:
                ChangeState(SmasherMobState.Perimeter_3);
                break;
        }
    }
    
    // Debug -------------------------
    #if UNITY_EDITOR
    protected override void OnDrawGizmos()
    {
        if(!enableDebugging) return;
        base.OnDrawGizmos();
        var tr = transform;
        var pos = tr.position;
        
        Handles.color = Color.green;
        Handles.DrawLine(pos, pos + transform.forward * 10, 2);
        
        DebugDistance(tr, pos);

        if(Application.isPlaying)
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.red;
            style.fontSize = 25;
            Handles.Label(pos + new Vector3(0,5,0), $"State {smasherMobState}", style);
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
                    color = new Color32(0, 125, 255, 50); 
                    break;
                case 1:
                    color = new Color32(0, 125, 255, 30); 
                    break;
                case 2:
                    color = new Color32(0, 125, 255, 20); 
                    break;
                case 3:
                    color = new Color32(0, 125, 255, 10); 
                    break;
            }
            Handles.color = color;
            Handles.DrawSolidDisc(pos, tr.up, perimeters[i].distToEnemy);
        }
    }
    #endif
}

[System.Serializable]
public class Perimeters
{
    [Range(0, 120)] public float distToEnemy;
    [Range(0, 120)] public float timeSpentInPerimeter;
}
