using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Weapon
{
    [CreateAssetMenu(menuName = "Weapon Scriptable/Weapon_scriptable", fileName = "new Weapon")]
    public class SO_Weapon : ScriptableObject
    {
        [field: Header("-----Weapon-----")]
        [field: SerializeField] internal string weaponName{ get; set; }
        
        [field: SerializeField] internal bool isWeaponPossessByPlayer;
        [field: SerializeField] internal LayerMask hitLayer{ get; set; }
        [field: Tooltip("Which fire mode is selected")]
        [field: SerializeField] internal WeaponMode weaponModeIndex { get; private set; }
        [field: SerializeField] internal List<SO_WeaponMode> weaponMode;

    }
    
    public enum WeaponMode
    {
        Primary,
        Secondary
    }
}

