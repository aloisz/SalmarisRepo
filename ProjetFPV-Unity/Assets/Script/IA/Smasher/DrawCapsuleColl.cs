using System;
using System.Collections;
using System.Collections.Generic;
using MyAudio;
using NaughtyAttributes;
using UnityEngine;

namespace AI
{
    public class DrawCapsuleColl : MonoBehaviour, IDamage
    {
        private AI_Pawn pawn;
        [SerializeField] private Color32 color;
        [SerializeField][Range(1,5)] private float tickness;

        [SerializeField] private float damageMultiplier;

        [SerializeField]  private bool isWeakPoint;

        private bool _alreadyHitSound;

        private void Awake()
        {
            pawn = GetComponentInParent<AI_Pawn>();
        }

        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Helper.DrawWireCapsule(transform.position, transform.rotation, GetComponent<CapsuleCollider>().radius, 
                GetComponent<CapsuleCollider>().height, color,tickness);
        }
        #endif

        public void Hit(float damageInflicted)
        {
            pawn.actualPawnHealth -= damageInflicted * damageMultiplier;
            PlayerKillStreak.Instance.NotifyDamageInflicted();

            if (_alreadyHitSound) return;
            // audio
            AudioManager.Instance.SpawnAudio3D(transform.position, SfxType.SFX, isWeakPoint ? 5 : (damageMultiplier < 0.05f ? 4 : 6), 
                1, 0, 1);
            _alreadyHitSound = true;
            StartCoroutine(nameof(ResetHitSound));
        }

        private IEnumerator ResetHitSound()
        {
            yield return new WaitForSeconds(0.05f);
            _alreadyHitSound = false;
        }
    }

}
