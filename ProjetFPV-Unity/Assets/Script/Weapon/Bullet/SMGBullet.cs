using System.Collections;
using System.Collections.Generic;
using CameraBehavior;
using MyAudio;
using UnityEngine;

public class SMGBullet : BulletBehavior
{
    [Header("Bobbing Amount")] [SerializeField]
    private float amount = 10;
    [SerializeField] private float speed = 5;
    private float timer = 0;
    private AudioSource source;
    protected override void CollideWithPlayerMask(Collision collision)
    {
        base.CollideWithPlayerMask(collision);
    }

    protected override void CollideWithWalkableMask(Collision collision)
    {
        base.CollideWithWalkableMask(collision);
        AudioManager.Instance.SpawnAudio3D(collision.transform.position, SfxType.SFX, 21, .5f, 0, 1, 1,0,
            AudioRolloffMode.Logarithmic, 5,40);
    }
    
    protected override void Start()
    {
        source = GetComponent<AudioSource>();
        source.clip = AudioManager.Instance.audioSO[0].soundList[21 - 1].audioClip;
        source.loop = true;
        source.Play();
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
