using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;


namespace CameraBehavior
{
    [CreateAssetMenu(menuName = "Camera Scriptable/Camera_Scriptable", fileName = "new Camera Behavior")]
    public class SO_Camera : ScriptableObject
    {
        /// <summary>
        /// Cam Effect
        /// </summary>
        [field: BoxGroup("-----Cam Effect-----")][field: SerializeField] internal float positionOffSetSmooth { get; private set; }
        [field: BoxGroup("-----Cam Effect-----")][field: SerializeField] internal float rotationOffSetSmooth { get; private set; }
        
        /// <summary>
        /// Bobbing
        /// </summary>
        [field: BoxGroup("---Bobbing---")][field: Range(0, 20)] [field:SerializeField] internal float walkingBobbingSpeed { get; private set; }
        [field: BoxGroup("---Bobbing---")][field: Range(-.01f, .01f)] [field:SerializeField] internal float cameraBobbingAmount { get; private set; }
        [field: BoxGroup("---Bobbing---")][field: Range(-.03f, .03f)] [field:SerializeField] internal float weaponBobbingAmount { get; private set; }
        
        
        /// <summary>
        /// Idle
        /// </summary>
        [field: BoxGroup("---Idle---")][field: SerializeField] internal float timeToGetToIdleFov { get; private set; }
        [field: BoxGroup("---Idle---")][field: SerializeField] internal float fovIdle { get; private set; }
        
        
        /// <summary>
        /// Moving
        /// </summary>
        [field: BoxGroup("---Moving---")][field: SerializeField] internal float timeToGetToMovingFov { get; private set; }
        [field: BoxGroup("---Moving---")][field: SerializeField] internal float fovMoving { get; private set; }
        [field: BoxGroup("---Moving---")][field: SerializeField] internal Vector3 rotationOffSet { get; private set; }

       
        /// <summary>
        /// Sliding
        /// </summary>
        [field: BoxGroup("---Sliding---")][field: SerializeField] internal float slindingRotMultiplier { get; private set; }
        [field: BoxGroup("---Sliding---")][field: SerializeField] internal float timeToGetToSlidingFov { get; private set; }
        [field: BoxGroup("---Sliding---")][field: SerializeField] internal float fovSliding { get; private set; }
        
        
        /// <summary>
        /// Dashing
        /// </summary>
        [field: BoxGroup("---Dashing---")][field: SerializeField] internal float timeToGetToDashingFov { get; private set; }
        [field: BoxGroup("---Dashing---")][field: SerializeField] internal float fovDashing { get; private set; }
        
        [field: BoxGroup("---Dashing---")][field: SerializeField] internal float dashingRotMultiplier { get; private set; }
        [field: BoxGroup("---Dashing---")][field: SerializeField] internal Vector3 dashingRotationOffSet { get; private set; }
        
        
        /// <summary>
        /// Jumping
        /// </summary>
        [field: BoxGroup("---Jumping---")][field: Range(0, 50)] [field:SerializeField] internal float JumpingBobbingSpeed { get; private set; }
        [field: BoxGroup("---Jumping---")][field: Range(-.01f, 10)] [field:SerializeField] internal float cameraJumpingBobbingAmount { get; private set; }
        [field: BoxGroup("---Jumping---")][field: Range(0, 100)] [field:SerializeField] internal float highSpeedEnabled { get; private set; }
        [field: BoxGroup("---Jumping---")][field: Range(0, 5)] [field:SerializeField] internal float highSpeedMaxMultiplierValue{ get; private set; }
        [field: BoxGroup("---Jumping---")][field: Range(0, 5)] [field:SerializeField] internal float highSpeedMultiplier{ get; private set; }
        [field: BoxGroup("---Jumping---")][field: Range(0, 10)] [field:SerializeField] internal float highSpeedDeMultiplier { get; private set; }
        
        /// <summary>
        /// Weapon Sway Settings
        /// </summary>
        [field: BoxGroup("---Weapon Sway Settings---")][field: SerializeField] internal float weaponSwaySmooth { get; private set; }
        [field: BoxGroup("---Weapon Sway Settings---")][field: SerializeField] internal float weaponSwaymultiplier { get; private set; }
    }
}

