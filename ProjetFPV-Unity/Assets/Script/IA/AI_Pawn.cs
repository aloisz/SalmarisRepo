using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
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
        [SerializeField] protected Transform targetToFollow;
        [SerializeField] protected SO_IA so_IA;
        
        [Header("Pawn Properties")]
        public LayerMask targetMask;
        [Space] [ProgressBar("Health", 500, EColor.Red)] public float actualPawnHealth;
        [SerializeField] internal PawnState pawnState;
        
        [Header("Tick")]
        [SerializeField][Tooltip("How many time the check is performed")] protected float tickVerification = 0.2f;

        [Header("Director")]
        public float enemyWeight;
        
        [Header("--- Perimeter ---")] 
        public List<Perimeters> perimeters;
        
        //Component----------------------
        internal NavMeshAgent navMeshAgent;
        protected AgentLinkMover agentLinkMover;
        [HideInInspector] public Rigidbody rb;
        [Header("Vision Module")]
        [SerializeField] protected SphereCollider visionDetector;
        
        protected virtual void Start()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            agentLinkMover = GetComponent<AgentLinkMover>();
            rb = GetComponent<Rigidbody>();
            
            GetPawnPersonnalInformation();
            GameManager.Instance.aiPawnsAvailable.Add(this);
        }

        public virtual void GetPawnPersonnalInformation()
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

        protected virtual void ResetAgent()
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            
            GetPawnPersonnalInformation();
        }

        protected virtual void Update()
        {
            CheckIfIsStillAlive();
            //PawnAvoidance();
            TickHandler();
        }

        protected virtual void FixedUpdate()
        {
            rb.AddForce(Vector3.down * so_IA.gravityApplied); // Gravity apply to the agent
        }

        private float timer = 0;
        protected void TickHandler()
        {
            if (timer < tickVerification)
            {
                timer += Time.deltaTime;
                if (timer > tickVerification)
                {
                    timer = 0; 
                    
                    if(pawnState == PawnState.Disable) return;
                    if(!isFleeing) FollowTarget();
                    PawnBehavior();
                }
            }
        }
        protected virtual void PawnBehavior(){ }

        private void OnDisable()
        {
            targetToFollow = null;
        }

        private void CheckIfIsStillAlive()
        {
            if (actualPawnHealth <= 0)
            {
                DestroyLogic();
                if(Director.Instance) Director.Instance.TryAddingValueFromLastKilledEnemy(enemyWeight);
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
        
        protected bool isFleeing = false;
        protected virtual void FollowTarget ()
        {
            if(!targetToFollow || !navMeshAgent.enabled) return;
            SetTarget(targetToFollow.position);
        }
        
        protected virtual void SetTarget (Vector3 targetToFollow)
        {
            navMeshAgent.SetDestination(targetToFollow);
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            targetToFollow = PlayerController.Instance.transform;
        }
        
        #endregion

        public virtual void DisableAgent()
        {
            ChangeState(PawnState.Disable);
            IsPhysicNavMesh(false);
            StartCoroutine(DisableAgentCorountine());
        }

        internal void ChangeState(PawnState newState)
        {
            pawnState = newState;
        }

        private IEnumerator DisableAgentCorountine()
        {
            yield return new WaitForSeconds(so_IA.knockoutTime);
            ChangeState(PawnState.Enable);
            IsPhysicNavMesh(true);
        }
        
        /// <summary>
        /// Handles physics betweren Nav Mesh and Rigidbody
        /// </summary>
        /// <param name="condition">if true navmesh true, rb kinematic enable
        /// if false navmesg disable, rb kinematic disable </param>
        public void IsPhysicNavMesh(bool condition)
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

        public void EnableNavMesh(bool enabled)
        {
            if(navMeshAgent)navMeshAgent.enabled = enabled;
        }

        #if UNITY_EDITOR
        protected virtual void OnDrawGizmos()
        {
            //Vision Module
            Handles.color = Color.red;
            Handles.DrawWireArc(transform.position, transform.up, transform.right, 360, so_IA.visionDetectorRadius, 3);
            Handles.DrawWireArc(transform.position, transform.right, transform.up, 360, so_IA.visionDetectorRadius, 3);
            Handles.DrawWireArc(transform.position, transform.forward, transform.right, 360, so_IA.visionDetectorRadius, 3);
            
            var tr = transform;
            var pos = tr.position;
            DebugDistance(tr, pos);
        }
        
        protected virtual void DebugDistance(Transform tr, Vector3 pos)
        {
            for (int i = 0; i < perimeters.Count; i++)
            {
                Color32 color = new Color32();
                color = new Color32(0, 125, 255, (byte)i); 
                switch (i)
                {
                    case 0:
                        color = new Color32(0, 125, 255, 50); 
                        break;
                    case 1:
                        color = new Color32(0, 125, 255, 30); 
                        break;
                    case 2:
                        color = new Color32(0, 125, 255, 20); 
                        break;
                    case 3:
                        color = new Color32(0, 125, 255, 10); 
                        break;
                    case 4:
                        color = new Color32(0, 125, 255, 5); 
                        break;
                }
                Handles.color = color;
                Handles.DrawSolidDisc(pos, tr.up, perimeters[i].distToEnemy);
            }
        }
        
        #endif
    }
    
    
    internal enum PawnState
    {
        Enable,
        Disable
    }
}



