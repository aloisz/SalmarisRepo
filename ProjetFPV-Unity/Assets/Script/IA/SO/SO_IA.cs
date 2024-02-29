using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    [CreateAssetMenu(menuName = "IA Scriptable/IA_Scriptable", fileName = "new IA")]
    public class SO_IA : ScriptableObject
    {
        [field: Header("Movement Module")] 
        
        [field: Range(0, 200)][field:SerializeField] internal float accelerationSpeed { get; private set; }
        [field: Range(0, 50)][field:SerializeField] internal float walkingSpeed { get; private set; }
        [field: Range(0, 10)][field:SerializeField] internal float stoppingDistance { get; private set; }
        [field:Tooltip("Permit the pawn to turn more quiclky")][field: Range(0, 400)][field:SerializeField] internal float angularSpeed { get; private set; }
        
        [field: Header("Health Module")] 
        [field: Range(0, 500)][field:SerializeField] internal float health { get; private set; }

        [field: Header("Avoidance Module")]
        [field:Tooltip("Distance between each pawn")][field: Range(.5f, 10)][field:SerializeField] internal float avoidanceDistance { get; private set; }
        
        
        [field: Header("Vision Module")]
        [field:SerializeField] internal float visionDetectorRadius  { get; private set; }
    }
}
