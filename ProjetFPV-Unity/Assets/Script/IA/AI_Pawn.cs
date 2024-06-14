using System;
using System.Collections;
using System.Collections.Generic;
using MyAudio;
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
        [SerializeField] internal Transform targetToFollow;
        [SerializeField] public SO_IA so_IA;
        
        [Header("Pawn Properties")]
        public LayerMask targetMask;
        
        [Space] [ProgressBar("Health", 500, EColor.Red)] public float actualPawnHealth;
        [SerializeField] internal PawnState pawnState;
        [SerializeField] private EnemyToSpawn.EnemyKeys mobType;
        [SerializeField] protected Vector3 knockBackDeathIntensityXYZ;
        protected float knockBackMultiplier;
        public Action onEnemyDead;
        
        [Header("Tick")]
        [SerializeField][Tooltip("How many time the check is performed")] protected float tickVerification = 0.2f;
        [HideInInspector] protected float deathDelay = 2;

        [Header("Director")]
        public float enemyWeight;
        
        [Header("--- Perimeter ---")] 
        public List<Perimeters> perimeters;

        internal bool isPawnDead = false;
        
        //Component----------------------
        internal NavMeshAgent navMeshAgent;
        protected AgentLinkMover agentLinkMover;
        [HideInInspector] public Rigidbody rb;
        [Header("Vision Module")]
        [SerializeField] protected SphereCollider visionDetector;

        [Header("VFX")] 
        [SerializeField] private ParticleSystem VFXSpawn;
        private AI_Material[] _aiMaterials;
        
        protected virtual void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            agentLinkMover = GetComponent<AgentLinkMover>();
            rb = GetComponent<Rigidbody>();

            _aiMaterials = GetComponentsInChildren<AI_Material>();
            
            ResetAgent();
            //GameManager.Instance.aiPawnsAvailable.Add(this);
        }

        protected virtual void OnDisable()
        {
            navMeshAgent.enabled = false;
            rb.isKinematic = false;

            navMeshAgent.speed = so_IA.walkingSpeed;
            navMeshAgent.stoppingDistance = so_IA.stoppingDistance;
            navMeshAgent.acceleration = so_IA.accelerationSpeed;
            navMeshAgent.radius = so_IA.avoidanceDistance;
            navMeshAgent.angularSpeed = so_IA.angularSpeed;
            
            pawnState = PawnState.Disable;
            
            actualPawnHealth = so_IA.health;
            isPawnDead = true;

            visionDetector.isTrigger = false;
            visionDetector.radius = so_IA.visionDetectorRadius;
        }

        public void SpawnVFX()
        {
            RaycastHit hit;
            Physics.Raycast(transform.position, Vector3.down, out hit, 500f, PlayerController.Instance.groundLayer);
            var pos = hit.point;

            var vfx = Instantiate(VFXSpawn);
            vfx.transform.position = pos;
            vfx.transform.localScale *= 2f;
            vfx.Play();
        }

        //[SerializeField] private bool alreadyPlacedInScene;
        public virtual void ResetAgent()
        {
            //StartCoroutine(nameof(DelayedNavMesh));
            
            navMeshAgent.enabled = true;
            rb.isKinematic = true;

            navMeshAgent.speed = so_IA.walkingSpeed;
            navMeshAgent.stoppingDistance = so_IA.stoppingDistance;
            navMeshAgent.acceleration = so_IA.accelerationSpeed;
            navMeshAgent.radius = so_IA.avoidanceDistance;
            navMeshAgent.angularSpeed = so_IA.angularSpeed;

            pawnState = PawnState.Enable;

            if (_aiMaterials.Length > 0)
            {
                foreach(AI_Material aiMaterial in _aiMaterials) aiMaterial.Reset();
            }
            
            actualPawnHealth = so_IA.health;
            isPawnDead = false;

            visionDetector.isTrigger = true;
            visionDetector.radius = so_IA.visionDetectorRadius;

            targetToFollow = null;
        }

        IEnumerator DelayedNavMesh()
        {
            yield return new WaitForSeconds(0.05f);
            navMeshAgent.enabled = true;
            rb.isKinematic = true;
        }

        protected virtual void OldResetAgent()
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            
            ResetAgent();
        }

        protected virtual void Update()
        {
            if (isPawnDead) return;
            
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
            if(pawnState == PawnState.Disable) return;
            if (timer < tickVerification)
            {
                timer += Time.deltaTime;
                if (timer > tickVerification)
                {
                    timer = 0; 
                    if(!isFleeing) FollowTarget();
                    PawnBehavior();
                }
            }
        }
        protected virtual void PawnBehavior(){ }

        private void CheckIfIsStillAlive()
        {
            if (actualPawnHealth <= 0)
            {
                isPawnDead = true;
                DestroyLogic();
                
                onEnemyDead.Invoke();

                RaycastHit hit;
                Physics.Raycast(transform.position, Vector3.down, out hit, 500, PlayerController.Instance.groundLayer);
                DecalSpawnerManager.Instance.SpawnDecal(hit.point, hit.normal, "Enemy_Death_Decal");

                if (Director.Instance)
                {
                    Director.Instance.TryAddingValueFromLastKilledEnemy(enemyWeight);
                    Director.Instance.currentWaveIntensity -= enemyWeight;
                }
                if(PlayerKillStreak.Instance) PlayerKillStreak.Instance.NotifyEnemyKilled(mobType);

                if (_aiMaterials.Length > 0)
                {
                    deathDelay = _aiMaterials[0].deathDuration + _aiMaterials[0].deathDissolveDuration + _aiMaterials[0].dissolveDelay;
                    foreach (AI_Material aiMaterial in _aiMaterials)
                    {
                        aiMaterial.Death();
                    }
                }
            }
            else
            {
                isPawnDead = false;
            }
        }
        public virtual void DestroyLogic(){}

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
        public virtual void IsPhysicNavMesh(bool condition)
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

        private bool hitAudio = false;
        public virtual void Hit(float damageInflicted)
        {
            actualPawnHealth -= damageInflicted;
            knockBackMultiplier = Mathf.Clamp(damageInflicted, 1, 1.75f);
            PlayerKillStreak.Instance.NotifyDamageInflicted(damageInflicted);

            if (hitAudio) return;
            hitAudio = true;
            StartCoroutine(HitAudioCoroutine());
        }

        private IEnumerator HitAudioCoroutine()
        {
            HitAudio();
            yield return new WaitForSeconds(.1f);
            hitAudio = false;
        }
        
        /// <summary>
        /// Here put audio logic when pawn hit 
        /// </summary>
        protected virtual void HitAudio(){}

        public void EnableNavMesh(bool enabled)
        {
            if(navMeshAgent) navMeshAgent.enabled = enabled;
        }
        
        public void EnableNavMeshWithDelay(bool enabled, float delay = 0f)
        {
            StartCoroutine(Routine(enabled, delay));
        }
        IEnumerator Routine(bool enabled, float delay = 0f)
        {
            yield return new WaitForSeconds(delay);
            if(navMeshAgent) navMeshAgent.enabled = enabled;
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



