using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    [CreateAssetMenu(menuName = "Player Scriptable/PlayerScriptable", fileName = "new Player Scriptable")]
    public class PlayerScriptable : ScriptableObject
    {
        [Header("Movements")] public float moveSpeed = 1;
        public ForceMode movingMethod = ForceMode.Force;

        [Header("Physics")] public float friction = 1;
        public float linearDragMultiplier = 7;
        public float linearDragDeceleration = 3;

        [Header("Detection")] public Vector2 groundDetectionWidthAndHeight = Vector2.one;
        public Vector2 wallDetectionWidthAndHeight = Vector2.one;
    }
}
