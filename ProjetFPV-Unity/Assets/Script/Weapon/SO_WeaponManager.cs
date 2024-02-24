using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;



namespace Weapon
{
    [CreateAssetMenu(menuName = "Weapon Scriptable/Weapon_scriptable", fileName = "new Weapon")]
    public class SO_WeaponManager : ScriptableObject
    {
        [field: Header("-----Weapon-----")] 
        private int numberOfWeaponState;
        [field: SerializeField] internal string weaponName { get; private set; }
        [field: SerializeField] internal List<WeaponState> modeOfWeapon { get; private set; }
        
    }
}

[System.Serializable]
public class WeaponState 
{
    // Weapon State
    public int numberOfBullet;
    public float timeToReload;
    public float bulletSpeed;
    public float bulletDamage;
    public float fireRate;
    
    [Space]
    public SelectiveFireType selectiveFireState;
    public MunitionType munitionTypeState;

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
