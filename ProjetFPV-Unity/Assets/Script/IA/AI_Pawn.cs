using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;


namespace AI
{
    public class AI_Pawn : MonoBehaviour, IDamage
    {

        [SerializeField] private Transform targetToFollow;
        [SerializeField] protected SO_IA so_IA;

        protected float actualPawnHealth;
        
        //Component----------------------
        protected NavMeshAgent navMeshAgent;
        [SerializeField] protected SphereCollider visionDetector;
        
        protected virtual void Start()
        {
            GetPawnPersonnalInformation();
        }

        protected virtual void GetPawnPersonnalInformation()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();

            navMeshAgent.speed = so_IA.walkingSpeed;
            navMeshAgent.stoppingDistance = so_IA.stoppingDistance;
            navMeshAgent.acceleration = so_IA.accelerationSpeed;
            navMeshAgent.radius = so_IA.avoidanceDistance;

            actualPawnHealth = so_IA.health;

            visionDetector.isTrigger = true;
            visionDetector.radius = so_IA.visionDetectorRadius;
        }

        protected virtual void Update()
        {
            CheckIfIsStillAlive();
            FollowTarget();
        }
        
        protected virtual void CheckIfIsStillAlive ()
        {
            if (actualPawnHealth <= 0)
            {
                Destroy(gameObject);
            }
        }
        
        

        #region VisionModule

        protected virtual void FollowTarget ()
        {
            if(!targetToFollow) return;
            SetTarget(targetToFollow);
        }
        
        protected virtual void SetTarget (Transform targetToFollow)
        {
            navMeshAgent.SetDestination(targetToFollow.position);
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            targetToFollow = PlayerController.Instance.transform;
        }

        #endregion
        
        
        public void Hit(float damageInflicted)
        {
            actualPawnHealth -= damageInflicted;
        }


#if UNITY_EDITOR
        protected virtual void OnDrawGizmos()
        {
            Handles.color = Color.red;
            Handles.DrawWireArc(transform.position, transform.up, transform.right, 360, so_IA.visionDetectorRadius, 3);
            Handles.DrawWireArc(transform.position, transform.right, transform.up, 360, so_IA.visionDetectorRadius, 3);
            Handles.DrawWireArc(transform.position, transform.forward, transform.right, 360, so_IA.visionDetectorRadius, 3);
        }
        #endif
    }
}

