using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;


namespace AI
{
    [RequireComponent(typeof(SphereCollider))]
    public class AI_Pawn : MonoBehaviour
    {

        [SerializeField] private Transform targetToFollow;

        [Header("Movement Module")] 
        [SerializeField] protected float walkingSpeed;
        [SerializeField] protected float stoppingDistance;
        
        [Header("Vision Module")]
        [SerializeField] protected float visionDetectorRadius;
        
        //Component----------------------
        protected NavMeshAgent navMeshAgent;
        protected SphereCollider visionDetector;
        
        protected virtual void Start()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            visionDetector = GetComponent<SphereCollider>();

            visionDetector.isTrigger = true;
            visionDetector.radius = visionDetectorRadius;
        }

        protected virtual void Update()
        {
            if(!targetToFollow) return;
            navMeshAgent.SetDestination(targetToFollow.position);
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            targetToFollow = PlayerController.Instance.transform;
        }


#if UNITY_EDITOR
        protected virtual void OnDrawGizmos()
        {
            Handles.color = Color.red;
            Handles.DrawWireArc(transform.position, transform.up, transform.right, 360, visionDetectorRadius, 3);
            Handles.DrawWireArc(transform.position, transform.right, transform.up, 360, visionDetectorRadius, 3);
            Handles.DrawWireArc(transform.position, transform.forward, transform.right, 360, visionDetectorRadius, 3);
        }
        #endif
    }
}

