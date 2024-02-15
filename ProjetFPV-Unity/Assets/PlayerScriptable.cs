using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Player
{
    [CreateAssetMenu(menuName = "Player Scriptable/PlayerScriptable", fileName = "new Player Scriptable")]
    public class PlayerScriptable : ScriptableObject
    {
        [Header("Movements")] public float moveSpeed = 1;
        public float moveThreshold = 0.1f;
        public float jumpForce = 20f;
        public float accelerationSpeed = 20f;
        public float moveAirMultiplier = 0.1f;

        [Header("Physics")] public float friction = 1;
        public float gravityMultiplier = 1;

        [Header("Detection")] public Vector3 groundDetectionWidthHeightDepth = Vector3.one;
        public Vector2 wallDetectionWidthAndHeight = Vector2.one;

        [Header("View")] 
        public float lookSpeed = 1f;
        public float lookLimitX = 180f;
        public float smoothCameraPos = 15f;
        public float smoothCameraRot = 15f;
    }
}
