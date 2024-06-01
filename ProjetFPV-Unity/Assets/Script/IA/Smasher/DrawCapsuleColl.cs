using System;
using System.Collections;
using System.Collections.Generic;
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
        }
    }

}
