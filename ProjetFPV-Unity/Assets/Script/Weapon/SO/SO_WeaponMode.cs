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
        [field: SerializeField] internal float fireRate{ get; private set; }

        [field: Space] 
        [field: Header("-----Weapon Modificator-----")]
        
        #region Dispersion
        [field: Space]
        [field: SerializeField] internal bool isHavingDispersion;
        
        [field: ShowIf("isHavingDispersion")] [MinMaxSlider(1, 100.0f)] [field: SerializeField]
        internal Vector2Int howManyBulletShot;
        
        [field: ShowIf("isHavingDispersion")] [MinMaxSlider(-100, 100)] [field: SerializeField]
        internal Vector2 zAxisDispersion;
        [field: ShowIf("isHavingDispersion")] [MinMaxSlider(-100, 100)] [field: SerializeField]
        internal Vector2 yAxisDispersion;
        #endregion
        
        #region Projectile Curve
        [field: Space]
        [field: ShowIf("munitionTypeState", MunitionType.Projectile)] [field: SerializeField]
        internal bool isProjectileHaveCurve;
        
        [field: Header("-----Projectile Specs-----")]
        [field: ShowIf("munitionTypeState", MunitionType.Projectile)][field: SerializeField] internal GameObject bullet{ get; private set; }
        [field: ShowIf("munitionTypeState", MunitionType.Projectile)][field: SerializeField] internal float bulletSpeed{ get; private set; }
        
        [field: Header("-----Projectile Curve-----")]
        [field: ShowIf("isProjectileHaveCurve")] [field: SerializeField]
        internal AnimationCurve projectileAnimationCurve;
        [field: ShowIf("isProjectileHaveCurve")] [field: SerializeField]
        internal float animationCurveHeight;
        [field: ShowIf("isProjectileHaveCurve")] [field: SerializeField]
        internal float animationCurveDistance;
        [field: ShowIf("isProjectileHaveCurve")] [field: SerializeField]
        internal float animationCurveTimeToComplete;

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
}


