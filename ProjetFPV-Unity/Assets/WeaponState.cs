using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Weapon;

public class WeaponState : GenericSingletonClass<WeaponState>
{
    public Barbatos barbatos;
    public SO_Weapon defaultWeapon;

    public override void Awake()
    {
        base.Awake();
        
        var component = GetComponent<Barbatos>();
        defaultWeapon = Instantiate(component.so_Weapon);

        barbatos = component;
        barbatos.so_Weapon = defaultWeapon;
    }
}
