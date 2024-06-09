using System.Collections;
using System.Collections.Generic;
using CameraBehavior;
using UnityEngine;

public class SMGBullet : BulletBehavior
{
    [Header("Bobbing Amount")] [SerializeField]
    private float amount = 10;
    [SerializeField] private float speed = 5;
    private float timer = 0;
    protected override void CollideWithPlayerMask(Collision collision)
    {
        base.CollideWithPlayerMask(collision);
    }
    
    protected virtual void FixedUpdate()
    {
        base.FixedUpdate();
        BulletSwing();
    }

    private void BulletSwing()
    {
        timer += Time.deltaTime * speed;
        Vector3 particle = transform.position;
        Vector3 particleBobbingPos = new Vector3(Mathf.Sin(-timer) * amount + particle.x, Mathf.Sin(-timer) * amount + particle.y,
            particle.z);
        
        
        transform.position = Vector3.Lerp(particle, particleBobbingPos, timer);
    }
}
