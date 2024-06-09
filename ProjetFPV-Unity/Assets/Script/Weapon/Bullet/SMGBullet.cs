using System.Collections;
using System.Collections.Generic;
using CameraBehavior;
using UnityEngine;

public class SMGBullet : BulletBehavior
{
    
    protected override void CollideWithPlayerMask(Collision collision)
    {
        base.CollideWithPlayerMask(collision);
        
    }
}
