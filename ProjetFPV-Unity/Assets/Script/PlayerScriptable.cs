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
        [Header("Movement")]
        [Tooltip("The player's movement speed.")] 
        public float moveSpeed = 1;
        
        [Tooltip("The player's movement speed will be divided by X in the air.")] 
        public float moveSpeedInAirDivider = 1;
        
        [Tooltip("The min speed magnitude where the player is considered moving.")] 
        public float moveThreshold = 0.1f;
        
        [Tooltip("The player's jump impulsion force.")]
        public float jumpForce = 20f;
        
        [Tooltip("The player's secondary jump impulsion force based on the Y velocity")]
        public float secondaryJumpMultiplierFromYVel = 20f;
        
        [Tooltip("The player's movement air multiplier.")]
        public float moveAirMultiplier = 0.1f;
        
        [Tooltip("The player's dash impulsion force.")]
        public float dashForce = 5f;
        
        [Tooltip("The player's dash duration, time before the player start to fall again.")]
        public float dashDuration = 0.5f;
        
        [Tooltip("The player's dash duration between each dash.")]
        public float dashLagDuration = 0.5f;
        
        [Tooltip("The player's dash speed multiplier. Example : 150% = 1.5")]
        public float dashSpeedMultiplier = 1.5f;
        
        [Tooltip("The player's dash speed multiplier duration.")]
        public float dashSpeedMultiplierDuration = 1.5f;
        
        [Tooltip("The player's dash speed multiplier reset time, the time for the value to reset.")]
        public float dashSpeedMultiplierResetDuration = 2f;
        
        [Tooltip("Under this speed magnitude value, the rigidbody velocity will be boosted.")]
        public float speedMaxToAccelerate = 12f;
        
        [Tooltip("Rigidbody's velocity acceleration under the speedMaxToAccelerate value.")]
        public float accelerationMultiplier = 1.2f;
        
        [Tooltip("Rigidbody's velocity force applied down when sliding a slope.")]
        public float slidingInSlopeDownForce = 200f;
        
        [Tooltip("Rigidbody's velocity force applied toward the slope when sliding it.")]
        public float slidingInSlopeLimiter = 2f;
        
        [Tooltip("Rigidbody's velocity limitation while moving and sliding in a slope")]
        public float overallMomentumLimiterMoveSlideInSlope = 10f;

        [Tooltip("The deceleration amount of the player's speed when he is climbing a slope while sliding.")] 
        public float decelerationMultiplierSlideInSlopeUp = 1000f;
        
        [Tooltip("")]
        public float jumpEdgeImpulseForce = 15f;
        
        [Tooltip("")]
        public AnimationCurve slideBoostCurve;

        //----------------------------------------------------
        
        [Header("Stamina")]
        [Tooltip("The stamina's value generated per second, from 0 to 1.")]
        public float staminaPerSecond = 0.005f;

        //----------------------------------------------------
        
        [Header("Physics")]
        [Tooltip("The gravity's multiplier while in the air.")]
        public float gravityMultiplier = 1;
        
        [Tooltip("The Rigidbody's drag while the player is on ground.")]
        public float groundDrag = 7;
        
        [Tooltip("The Rigidbody's drag while the player is in the air.")]
        public float airDrag = 1.8f;
        
        [Tooltip("The Rigidbody's max velocity.")] 
        public float maxRigidbodyVelocity = 100f;

        //----------------------------------------------------
        
        [Header("Detections")]
        [Tooltip("The BoxCast's dimension to detect ground from the player.")]
        public float groundDetectionLenght = 1f;
        
        [Tooltip("Raycast lenght to detect underneath the player's foot.")]
        public float raycastLenghtSlopeDetection = 1f;
        
        [Tooltip("The BoxCast's Z offset.")]
        public float groundDetectionForwardOffset = 1.2f;
        
        [Tooltip("The BoxCast's Y offset.")]
        public float groundDetectionUpOffset = 1f;

        [Tooltip("The minimum slope degree for be considered as a slope.")]
        public float minSlopeDegrees = 25f;
        
        [Tooltip("")]
        public float edgeDetectionLenght = 1f;
        
        [Tooltip("")]
        public float edgeDetectionBottomOffsetY = 0f;

        [Tooltip("")]
        public float edgeDetectionTopOffsetY = 0f;
        
        [Tooltip("")]
        public float edgeDetectionHeight = 1f;
        
        [Tooltip("")]
        public float edgeDetectionOffsetLenght = 0.2f;
        
        [Tooltip("")]
        public float maxHeightToJumpFacility = 5f;
        
        [Tooltip("Min and Max value where the player will be pushed when climbing an edge. " +
                 "More the edge is high, more the value will be near Max, less it is, more the value will be enar Min.")]
        public Vector2 minMaxJumpFacility = Vector2.zero;
        
        [Tooltip("")]
        public bool enableAutoJumpEdge = true;

        //----------------------------------------------------
        
        [Header("Look")]
        [Tooltip("The mouse's sensibility.")]
        public float sensibility = 1f;
        
        [Tooltip("The mouse's look axis Y limit.")]
        public float lookLimitY = 180f;
        
        //----------------------------------------------------
        
        [Header("Physical Material")]
        [Tooltip("The friction material to apply when you're idling.")]
        public PhysicMaterial frictionMaterial;
        
        [Tooltip("The friction material to apply when you're hitting a wall.")]
        public PhysicMaterial wallMaterial;
        
        [Tooltip("The friction material to apply when you're moving")]
        public PhysicMaterial movingMaterial;
        
        //-----------------------------------------------------
        
        [Header("States")]
        [Tooltip("The time before the player is considered idling.")]
        public float timeBeforeDetectedIdle = 1f;
        
        //-----------------------------------------------------
        
        [Header("Health")]
        [Tooltip("")]
        public float maxPlayerHealth = 1f;
        
        [Tooltip("")]
        public float maxPlayerShield = 1f;
        
    }
}
