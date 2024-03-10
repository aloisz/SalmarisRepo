using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Weapon;

public interface IRaycast
{
    public RaycastType ChooseRaycastType(RaycastType raycastType);
}
