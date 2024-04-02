using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Weapon.Interface;
using Random = UnityEngine.Random;


namespace AI
{
    public class AI_Pawn : MonoBehaviour, IDamage
    {
        [SerializeField] private Transform targetToFollow;
        [SerializeField] protected SO_IA so_IA;

        protected float actualPawnHealth;
        
        //Component----------------------
        protected NavMeshAgent navMeshAgent;
        protected AgentLinkMover agentLinkMover;
        protected Rigidbody rb;
        [SerializeField] protected SphereCollider visionDetector;
        
        protected virtual void Start()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            agentLinkMover = GetComponent<AgentLinkMover>();
            rb = GetComponent<Rigidbody>();
            
            GetPawnPersonnalInformation();
            GameManager.Instance.aiPawnsAvailable.Add(this);
        }

        protected virtual void GetPawnPersonnalInformation()
        {
            navMeshAgent.enabled = true;
            rb.isKinematic = true;

            navMeshAgent.speed = so_IA.walkingSpeed;
            navMeshAgent.stoppingDistance = so_IA.stoppingDistance;
            navMeshAgent.acceleration = so_IA.accelerationSpeed;
            navMeshAgent.radius = so_IA.avoidanceDistance;
            navMeshAgent.angularSpeed = so_IA.angularSpeed;

            actualPawnHealth = so_IA.health;

            visionDetector.isTrigger = true;
            visionDetector.radius = so_IA.visionDetectorRadius;
        }

        protected virtual void Update()
        {
            CheckIfIsStillAlive();
            //PawnAvoidance();
            FollowTarget();
        }

        private void OnDisable()
        {
            targetToFollow = null;
        }

        private void CheckIfIsStillAlive()
        {
            if (actualPawnHealth <= 0)
            {
                DestroyLogic();
            }
        }
        protected virtual void DestroyLogic(){}

        #region Avoidance Module

        protected void PawnAvoidance()
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, 10))
            {
                Debug.DrawRay(transform.position, transform.forward * 10, Color.yellow);
                if (hit.transform.GetComponent<AI_Pawn>() != null)
                {
                    transform.GetComponent<Rigidbody>().AddForce(transform.right * 50, ForceMode.Impulse);
                }
            }
        }

        #endregion

        #region VisionModule

        protected virtual void FollowTarget ()
        {
            if(!targetToFollow || !navMeshAgent.enabled) return;
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

        public virtual void DisableAgent()
        {
            IsPhysicNavMesh(false);
        }

        /// <summary>
        /// Handles physics betweren Nav Mesh and Rigidbody
        /// </summary>
        /// <param name="condition">if true navmesh true, rb kinematic enable
        /// if false navmesg disable, rb kinematic disable </param>
        protected void IsPhysicNavMesh(bool condition)
        {
            if (condition)
            {
                navMeshAgent.enabled = true;
                rb.isKinematic = true;
            }
            else
            {
                navMeshAgent.enabled = false;
                rb.isKinematic = false;
            }
        }
        
        public void Hit(float damageInflicted)
        {
            actualPawnHealth -= damageInflicted;
        }
        

        #if UNITY_EDITOR
        protected virtual void OnDrawGizmos()
        {
            //Vision Module
            Handles.color = Color.red;
            Handles.DrawWireArc(transform.position, transform.up, transform.right, 360, so_IA.visionDetectorRadius, 3);
            Handles.DrawWireArc(transform.position, transform.right, transform.up, 360, so_IA.visionDetectorRadius, 3);
            Handles.DrawWireArc(transform.position, transform.forward, transform.right, 360, so_IA.visionDetectorRadius, 3);
        }
        #endif
    }
}

