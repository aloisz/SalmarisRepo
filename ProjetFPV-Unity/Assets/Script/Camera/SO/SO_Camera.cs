using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;


namespace CameraBehavior
{
    [CreateAssetMenu(menuName = "Camera Scriptable/Camera_Scriptable", fileName = "new Camera Behavior")]
    public class SO_Camera : ScriptableObject
    {
        [field: Header("-----Cam Effect-----")]
        [field: SerializeField] internal float positionOffSetSmooth { get; private set; }
        [field: SerializeField] internal float rotationOffSetSmooth { get; private set; }
        
        [field: Header("---Bobbing---")]
        [field: Range(0, 20)] [field:SerializeField] internal float walkingBobbingSpeed { get; private set; }
        [field: Range(-.01f, .01f)] [field:SerializeField] internal float cameraBobbingAmount { get; private set; }
        [field: Range(-.03f, .03f)] [field:SerializeField] internal float weaponBobbingAmount { get; private set; }
        
        [field: Header("---Idle---")]
        [field: SerializeField] internal float timeToGetToTheNewFOV { get; private set; }
        [field: SerializeField] internal float fovIdle { get; private set; }

        [field: Header("---Moving---")]
        [field: SerializeField] internal float fovMoving { get; private set; }
        [field: SerializeField] internal Vector3 rotationOffSet { get; private set; }

        [field: Header("---Sliding---")] 
        [field: SerializeField] internal float slindingRotMultiplier { get; private set; }
        
        [field: Header("---Weapon Sway Settings---")]
        [field: SerializeField] internal float weaponSwaySmooth { get; private set; }
        [field: SerializeField] internal float weaponSwaymultiplier { get; private set; }
    }
}

