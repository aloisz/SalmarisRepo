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
        public float moveThreshold = 0.1f;
        public float jumpForce = 20f;
        public ForceMode movingMethod = ForceMode.Force;
        public ForceMode jumpMethod = ForceMode.Force;

        [Header("Physics")] public float friction = 1;
        public float linearDragMultiplier = 7;
        public float linearDragDeceleration = 3;
        public float gravityMultiplier = 3;
        public float airMomentum = 3;
        public AnimationCurve gravityJumpModify;

        [Header("Detection")] public Vector3 groundDetectionWidthHeightDepth = Vector3.one;
        public Vector2 wallDetectionWidthAndHeight = Vector2.one;

        [Header("View")] 
        public float lookSpeed = 1f;
        public float lookLimitX = 180f;
        public float smoothCameraPos = 15f;
        public float smoothCameraRot = 15f;
    }
}
