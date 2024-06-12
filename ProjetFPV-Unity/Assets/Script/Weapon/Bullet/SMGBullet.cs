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

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);
        source.clip = AudioManager.Instance.audioSO[0].soundList[22 - 1].audioClip;
        source.Play();
    }
    
    protected override void Start()
    {
        source = GetComponent<AudioSource>();
        source.clip = AudioManager.Instance.audioSO[0].soundList[21 - 1].audioClip;
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
