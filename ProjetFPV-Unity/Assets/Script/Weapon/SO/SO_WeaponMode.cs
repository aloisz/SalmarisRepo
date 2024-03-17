using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;


namespace Weapon
{
    [CreateAssetMenu(menuName = "Weapon Scriptable/WeaponMode_scriptable", fileName = "new WeaponMode")]
    public class SO_WeaponMode : ScriptableObject
    {
        [field: Header("-----Weapon Specs-----")] 
        [field: SerializeField] internal SelectiveFireType selectiveFireState{ get; private set; }
        [field: SerializeField] internal MunitionType munitionTypeState{ get; private set; }
        
        [field: Space]
        [field: Header("-----Base Modification-----")] 
        [field: SerializeField] internal int numberOfBullet{ get; private set; }
        [field: SerializeField] internal float timeToReload{ get; private set; }
        [field: SerializeField] internal float bulletDamage{ get; private set; }
        [field: Tooltip("Fire rate per second")][field: SerializeField] internal float fireRate{ get; private set; }
        [field: ShowIf("selectiveFireState", SelectiveFireType.Burst)] [field: SerializeField] internal int burstAmount{ get; private set; }
        [field: ShowIf("selectiveFireState", SelectiveFireType.Burst)] [field: SerializeField] internal float burstTime{ get; private set; }

        [field: Space] 
        #region Raycast
        // RAY DISTANCE
        #region Ray Distance
        [field: Header("-----Ray Distance-----")] 
        [field: ShowIf("munitionTypeState", MunitionType.Raycast)][field: SerializeField] internal bool isRayDistanceNotInfinte{ get; private set; }
        [field: ShowIf("isRayDistanceNotInfinte")][field: SerializeField][field: Range(0,1000)] internal float RayDistance{ get; private set; } 
        
        // RAY Cast Type
        [field: Header("-----Raycast Type-----")] 
        [field: ShowIf("munitionTypeState", MunitionType.Raycast)][field: SerializeField] internal RaycastType raycastType{ get; private set; }
        
        // RAY Radius
        [field: Header("-----RAY Radius-----")] 
        //[field: ShowIf("raycastType", RaycastType.SphereCast)]
        [field: ShowIf(EConditionOperator.And,"munitionTypeState", "raycastType" )]
        [field: SerializeField] internal float sphereCastRadius{ get; private set; }
        
        
        // DISPERSION
        #region Dispersion
        [field: Space]
        [field: Header("-----Dispersion-----")] 
        [field: SerializeField] internal bool isHavingDispersion{ get; private set; }
        
        
        [field: ShowIf("isHavingDispersion")] [MinMaxSlider(1, 100.0f)] [field: SerializeField]
        internal Vector2Int howManyBulletShot;
        
        [field: ShowIf("isHavingDispersion")] [MinMaxSlider(-100, 100)] [field: SerializeField]
        internal Vector2 zAxisDispersion;
        [field: ShowIf("isHavingDispersion")] [MinMaxSlider(-100, 100)] [field: SerializeField]
        internal Vector2 yAxisDispersion;
        #endregion

        #endregion
        #endregion
        
        #region Projectile
        // PORJECTILE CURVE
        #region Projectile Curve
        [field: Space]
        [field: Header("-----Projectile Curve-----")] 
        [field: ShowIf("munitionTypeState", MunitionType.Projectile)] [field: SerializeField]
        internal bool isProjectileHaveCurve;
        
        [field: ShowIf("munitionTypeState", MunitionType.Projectile)][field: SerializeField] internal GameObject bullet{ get; private set; }
        [field: ShowIf("munitionTypeState", MunitionType.Projectile)][field: SerializeField] internal float bulletSpeed{ get; private set; }
        
        [field: ShowIf("isProjectileHaveCurve")] [field: SerializeField]
        internal AnimationCurve projectileAnimationCurve;
        [field: ShowIf("isProjectileHaveCurve")] [field: SerializeField]
        internal float animationCurveHeight;
        [field: ShowIf("isProjectileHaveCurve")] [field: SerializeField]
        internal float animationCurveDistance;
        [field: ShowIf("isProjectileHaveCurve")] [field: SerializeField]
        internal float animationCurveTimeToComplete;

        #endregion

        #endregion

        #region Commun

        // Do Explosion
        [field: Header("-----Explosion-----")] 
        [field: SerializeField] internal bool doExplosion{ get; private set; }
        [field: ShowIf("doExplosion")][field: SerializeField] [field: MinMaxSlider(1, 100.0f)] internal Vector2 explosionRadius{ get; private set; }
        [field: ShowIf("doExplosion")][field: SerializeField] [field: MinMaxSlider(1, 100.0f)] internal Vector2 explosionDamage{ get; private set; }
        #endregion
    }
    
    public enum SelectiveFireType
    {
        Single,
        Burst,
        Auto
    }

    public enum MunitionType
    {
        Raycast,
        Projectile
    }
    
    public enum RaycastType
    {
        Raycast,
        SphereCast
    }
}


