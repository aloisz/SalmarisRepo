using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace Player
{
    [CreateAssetMenu(menuName = "Player Scriptable/PlayerScriptable", fileName = "new Player Scriptable")]
    public class PlayerScriptable : ScriptableObject
    {
        [Header("Movements")] public float moveSpeed = 1;
        [InfoBox("If direction.magnitude > this value, then the player is considered moving")] 
        public float moveThreshold = 0.1f;
        public ForceMode movingMethod = ForceMode.Force;

        [Header("Physics")] public float friction = 1;
        public float linearDragMultiplier = 7;
        public float linearDragDeceleration = 3;

        [Header("Detection")] public Vector2 groundDetectionWidthAndHeight = Vector2.one;
        public Vector2 wallDetectionWidthAndHeight = Vector2.one;

        [Header("View")] 
        public float lookSpeed = 1f;
        public float lookLimitX = 180f;
    }
}
