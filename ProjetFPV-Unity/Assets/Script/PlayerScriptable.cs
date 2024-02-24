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
        [BoxGroup("Movement")][Tooltip("The player's movement speed.")] 
        public float moveSpeed = 1;
        
        [BoxGroup("Movement")][Tooltip("The min speed magnitude where the player is considered moving.")] 
        public float moveThreshold = 0.1f;
        
        [BoxGroup("Movement")][Tooltip("The player's jump impulsion force.")]
        public float jumpForce = 20f;
        
        [BoxGroup("Movement")][Tooltip("The player's movement air multiplier.")]
        public float moveAirMultiplier = 0.1f;
        
        [BoxGroup("Movement")][Tooltip("The player's dash impulsion force.")]
        public float dashForce = 5f;
        
        [BoxGroup("Movement")][Tooltip("The player's dash duration, time before the player start to fall again.")]
        public float dashDuration = 0.5f;
        
        [BoxGroup("Movement")][Tooltip("The player's dash speed multiplier. Example : 150% = 1.5")]
        public float dashSpeedMultiplier = 1.5f;
        
        [BoxGroup("Movement")][Tooltip("The player's dash speed multiplier duration.")]
        public float dashSpeedMultiplierDuration = 1.5f;
        
        [BoxGroup("Movement")][Tooltip("The player's dash speed multiplier reset time, the time for the value to reset.")]
        public float dashSpeedMultiplierResetDuration = 2f;
        
        [BoxGroup("Movement")][Tooltip("Above this speed magnitude value, the player doesnt have friction.")]
        public float speedMaxToNoFriction = 0.1f;
        
        [BoxGroup("Movement")][Tooltip("Under this speed magnitude value, the rigidbody velocity will be boosted.")]
        public float speedMaxToAccelerate = 12f;
        
        [BoxGroup("Movement")][Tooltip("Rigidbody's velocity acceleration under the speedMaxToAccelerate value.")]
        public float accelerationMultiplier = 1.2f;
        
        [BoxGroup("Movement")][Tooltip("How many speed removed when falling on a slope.")]
        public float speedDuringSlopeFall = 1.2f;
        
        [BoxGroup("Movement")][Tooltip("How many speed removed when climbing a slope.")]
        public float speedDuringSlopeClimb = 1.2f;

        //----------------------------------------------------
        
        [BoxGroup("Stamina")][Tooltip("The stamina's value generated per second, from 0 to 1.")]
        public float staminaPerSecond = 0.005f;

        //----------------------------------------------------
        
        [BoxGroup("Physic")][Tooltip("The gravity's multiplier while in the air.")]
        public float gravityMultiplier = 1;
        
        [BoxGroup("Physic")][Tooltip("The Rigidbody's drag while the player is on ground.")]
        public float groundDrag = 7;
        
        [BoxGroup("Physic")][Tooltip("The Rigidbody's drag while the player is in the air.")]
        public float airDrag = 1.8f;

        //----------------------------------------------------
        
        [BoxGroup("Detection")][Tooltip("The BoxCast's dimension to detect ground from the player.")]
        public Vector3 groundDetectionWidthHeightDepth = Vector3.one;
        
        [BoxGroup("Detection")][Tooltip("Raycast lenght to detect underneath the player's foot.")]
        public float raycastLenghtSlopeDetection = 1f;

        [BoxGroup("Detection")][Tooltip("The minimum slope degree for be considered as a slope.")]
        public float minSlopeDegrees = 25f;
        
        //----------------------------------------------------
        
        [BoxGroup("Look")][Tooltip("The mouse's sensibility.")]
        public float sensibility = 1f;
        
        [BoxGroup("Look")][Tooltip("The mouse's look axis Y limit.")]
        public float lookLimitY = 180f;
        
        //----------------------------------------------------
        
        [BoxGroup("Physical Material")][Tooltip("The friction material to apply when you're idling.")]
        public PhysicMaterial frictionMaterial;
        
        [BoxGroup("Physical Material")][Tooltip("The friction material to apply when you're hitting a wall.")]
        public PhysicMaterial wallMaterial;
        
        [BoxGroup("Physical Material")][Tooltip("The friction material to apply when you're moving")]
        public PhysicMaterial movingMaterial;
        
        //-----------------------------------------------------
        
        [BoxGroup("States")][Tooltip("The time before the player is considered idling.")]
        public float timeBeforeDetectedIdle = 1f;
    }
}
