using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VladBullet : BulletBehavior
{
    public bool isBulletOnFire;
    private Color baseColor = Color.green;
    private Color onFireColor = Color.red;
    
    // component
    private Renderer renderer;

    protected override void Awake()
    {
        base.Awake();
        renderer = GetComponent<Renderer>();
    }

    public void IsBulletOnFire(bool isOnFire)
    {
        renderer.material.color = isOnFire ? onFireColor : baseColor;
    }
}
