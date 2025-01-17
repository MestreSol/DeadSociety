

using System.Collections.Generic;
using Core;
using Core.Managers;
using Core.Shared;
using Core.Utils;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace AI
{
    [RequireComponent(typeof(Health), typeof(Actor), typeof(NavMeshAgent))]
    public class EnemyController : MonoBehaviour
    {
        [System.Serializable]
        public struct RendererIndexData
        {
            public Renderer renderer;
            public int materialIndex;

            public RendererIndexData(Renderer renderer, int index)
            {
                this.renderer = renderer;
                materialIndex = index;
            }
        }

        [Header("Settings")]
        public float selfDestructYHeight = -20f;
        public float pathReachingRadius = 2f;
        public float orientationSpeed = 10f;
        public float deathDuration = 0f;
        public bool swapToNextWeapon = false;
        public float delayAfterWeaponSwap = 0f;

        [Header("Materials and Colors")]
        public Material eyeColorMaterial;
        [ColorUsage(true, true)]
        public Color defaultEyeColor;
        [ColorUsage(true, true)]
        public Color attackEyeColor;
        public Material bodyMaterial;
        [GradientUsage(true)]
        public Gradient onHitBodyGradient;
        public float flashOnHitDuration = 0.5f;

        [Header("Audio and VFX")]
        public AudioClip damageTick;
        public GameObject deathVfx;
        public Transform deathVfxSpawnPoint;
        public GameObject lootPrefab;
        [Range(0, 1)]
        public float dropRate = 1f;

        [Header("Gizmos Colors")]
        public Color pathReachingRangeColor = Color.yellow;
        public Color attackRangeColor = Color.red;
        public Color detectionRangeColor = Color.blue;

        [Header("Events")]
        public UnityAction onAttack;
        public UnityAction onDetectedTarget;
        public UnityAction onLostTarget;
        public UnityAction onDamaged;

        private List<RendererIndexData> bodyRenderers = new List<RendererIndexData>();
        private MaterialPropertyBlock bodyFlashMaterialPropertyBlock;
        private float lastTimeDamaged = float.NegativeInfinity;

        private RendererIndexData eyeRendererData;
        private MaterialPropertyBlock eyeColorMaterialPropertyBlock;

        public PatrolPath patrolPath { get; set; }
        public GameObject knownDetectedTarget => detectionModule.knownDetectedTarget;
        public bool isTargetInAttackRange => detectionModule.isTargetInAttackRange;
        public bool isSeeingTarget => detectionModule.isSeeingTarget;
        public bool hadKnownTarget => detectionModule.hadKnownTarget;
        public NavMeshAgent navMeshAgent { get; private set; }
        public DetectionModule detectionModule { get; private set; }

        private int pathDestinationNodeIndex;
        private EnemyManager enemyManager;
        private ActorsManager actorsManager;
        private Health health;
        private Actor actor;
        private Collider[] selfColliders;
        private GameFlowManager gameFlowManager;
        private bool wasDamagedThisFrame;
        private float lastTimeWeaponSwapped = float.NegativeInfinity;
        private int currentWeaponIndex;
        private WeaponController currentWeapon;
        private WeaponController[] weapons;
        private NavigationModule navigationModule;

        void Start()
        {
            // Initialize managers and components
            enemyManager = FindObjectOfType<EnemyManager>();
            DebugUtility.HandleErrorIfNullFindObject<EnemyManager, EnemyController>(enemyManager, this);

            actorsManager = FindObjectOfType<ActorsManager>();
            DebugUtility.HandleErrorIfNullFindObject<ActorsManager, EnemyController>(actorsManager, this);

            enemyManager.RegisterEnemy(this);

            health = GetComponent<Health>();
            DebugUtility.HandleErrorIfNullGetComponent<Health, EnemyController>(health, this, gameObject);

            actor = GetComponent<Actor>();
            DebugUtility.HandleErrorIfNullGetComponent<Actor, EnemyController>(actor, this, gameObject);

            navMeshAgent = GetComponent<NavMeshAgent>();
            selfColliders = GetComponentsInChildren<Collider>();

            gameFlowManager = FindObjectOfType<GameFlowManager>();
            DebugUtility.HandleErrorIfNullFindObject<GameFlowManager, EnemyController>(gameFlowManager, this);

            health.OnDie += OnDie;
            health.OnDamaged += OnDamaged;

            // Initialize weapons
            FindAndInitializeAllWeapons();
            var weapon = GetCurrentWeapon();
            weapon.ShowWeapon(true);

            // Initialize detection module
            var detectionModules = GetComponentsInChildren<DetectionModule>();
            DebugUtility.HandleErrorIfNoComponentFound<DetectionModule, EnemyController>(detectionModules.Length, this, gameObject);
            DebugUtility.HandleWarningIfDuplicateObjects<DetectionModule, EnemyController>(detectionModules.Length, this, gameObject);
            detectionModule = detectionModules[0];
            detectionModule.onDetectedTarget += OnDetectedTarget;
            detectionModule.onLostTarget += OnLostTarget;
            onAttack += detectionModule.OnAttack;

            // Initialize navigation module
            var navigationModules = GetComponentsInChildren<NavigationModule>();
            DebugUtility.HandleWarningIfDuplicateObjects<DetectionModule, EnemyController>(detectionModules.Length, this, gameObject);
            if (navigationModules.Length > 0)
            {
                navigationModule = navigationModules[0];
                navMeshAgent.speed = navigationModule.MoveSpeed;
                navMeshAgent.angularSpeed = navigationModule.AngularSpeed;
                navMeshAgent.acceleration = navigationModule.Acceleration;
            }

            // Initialize renderers
            foreach (var renderer in GetComponentsInChildren<Renderer>(true))
            {
                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    if (renderer.sharedMaterials[i] == eyeColorMaterial)
                    {
                        eyeRendererData = new RendererIndexData(renderer, i);
                    }

                    if (renderer.sharedMaterials[i] == bodyMaterial)
                    {
                        bodyRenderers.Add(new RendererIndexData(renderer, i));
                    }
                }
            }

            bodyFlashMaterialPropertyBlock = new MaterialPropertyBlock();

            if (eyeRendererData.renderer != null)
            {
                eyeColorMaterialPropertyBlock = new MaterialPropertyBlock();
                eyeColorMaterialPropertyBlock.SetColor("_EmissionColor", defaultEyeColor);
                eyeRendererData.renderer.SetPropertyBlock(eyeColorMaterialPropertyBlock, eyeRendererData.materialIndex);
            }
        }

        void Update()
        {
            EnsureIsWithinLevelBounds();

            detectionModule.HandleTargetDetection(actor, selfColliders);

            Color currentColor = onHitBodyGradient.Evaluate((Time.time - lastTimeDamaged) / flashOnHitDuration);
            bodyFlashMaterialPropertyBlock.SetColor("_EmissionColor", currentColor);
            foreach (var data in bodyRenderers)
            {
                data.renderer.SetPropertyBlock(bodyFlashMaterialPropertyBlock, data.materialIndex);
            }

            wasDamagedThisFrame = false;
        }

        void EnsureIsWithinLevelBounds()
        {
            if (transform.position.y < selfDestructYHeight)
            {
                Destroy(gameObject);
                return;
            }
        }

        void OnLostTarget()
        {
            onLostTarget.Invoke();

            if (eyeRendererData.renderer != null)
            {
                eyeColorMaterialPropertyBlock.SetColor("_EmissionColor", defaultEyeColor);
                eyeRendererData.renderer.SetPropertyBlock(eyeColorMaterialPropertyBlock, eyeRendererData.materialIndex);
            }
        }

        void OnDetectedTarget()
        {
            onDetectedTarget.Invoke();

            if (eyeRendererData.renderer != null)
            {
                eyeColorMaterialPropertyBlock.SetColor("_EmissionColor", attackEyeColor);
                eyeRendererData.renderer.SetPropertyBlock(eyeColorMaterialPropertyBlock, eyeRendererData.materialIndex);
            }
        }

        public void OrientTowards(Vector3 lookPosition)
        {
            Vector3 lookDirection = Vector3.ProjectOnPlane(lookPosition - transform.position, Vector3.up).normalized;
            if (lookDirection.sqrMagnitude != 0f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * orientationSpeed);
            }
        }

        bool IsPathValid()
        {
            return patrolPath && patrolPath.PathNodes.Count > 0;
        }

        public void ResetPathDestination()
        {
            pathDestinationNodeIndex = 0;
        }

        public void SetPathDestinationToClosestNode()
        {
            if (IsPathValid())
            {
                int closestPathNodeIndex = 0;
                for (int i = 0; i < patrolPath.PathNodes.Count; i++)
                {
                    float distanceToPathNode = patrolPath.GetDistanceToNode(transform.position, i);
                    if (distanceToPathNode < patrolPath.GetDistanceToNode(transform.position, closestPathNodeIndex))
                    {
                        closestPathNodeIndex = i;
                    }
                }

                pathDestinationNodeIndex = closestPathNodeIndex;
            }
            else
            {
                pathDestinationNodeIndex = 0;
            }
        }

        public Vector3 GetDestinationOnPath()
        {
            if (IsPathValid())
            {
                return patrolPath.GetPositionOfPathNode(pathDestinationNodeIndex);
            }
            else
            {
                return transform.position;
            }
        }

        public void SetNavDestination(Vector3 destination)
        {
            if (navMeshAgent)
            {
                navMeshAgent.SetDestination(destination);
            }
        }

        public void UpdatePathDestination(bool inverseOrder = false)
        {
            if (IsPathValid())
            {
                if ((transform.position - GetDestinationOnPath()).magnitude <= pathReachingRadius)
                {
                    pathDestinationNodeIndex = inverseOrder ? (pathDestinationNodeIndex - 1) : (pathDestinationNodeIndex + 1);
                    if (pathDestinationNodeIndex < 0)
                    {
                        pathDestinationNodeIndex += patrolPath.PathNodes.Count;
                    }

                    if (pathDestinationNodeIndex >= patrolPath.PathNodes.Count)
                    {
                        pathDestinationNodeIndex -= patrolPath.PathNodes.Count;
                    }
                }
            }
        }

        void OnDamaged(float damage, GameObject damageSource)
        {
            if (damageSource && !damageSource.GetComponent<EnemyController>())
            {
                detectionModule.OnDamaged(damageSource);

                onDamaged?.Invoke();
                lastTimeDamaged = Time.time;

                if (damageTick && !wasDamagedThisFrame)
                    AudioUtility.CreateSFX(damageTick, transform.position, AudioUtility.AudioGroups.DamageTick, 0f);

                wasDamagedThisFrame = true;
            }
        }

        void OnDie()
        {
            var vfx = Instantiate(deathVfx, deathVfxSpawnPoint.position, Quaternion.identity);
            Destroy(vfx, 5f);

            enemyManager.UnregisterEnemy(this);

            if (TryDropItem())
            {
                Instantiate(lootPrefab, transform.position, Quaternion.identity);
            }

            Destroy(gameObject, deathDuration);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = pathReachingRangeColor;
            Gizmos.DrawWireSphere(transform.position, pathReachingRadius);

            if (detectionModule != null)
            {
                Gizmos.color = detectionRangeColor;
                Gizmos.DrawWireSphere(transform.position, detectionModule.detectionRange);

                Gizmos.color = attackRangeColor;
                Gizmos.DrawWireSphere(transform.position, detectionModule.attackRange);
            }
        }

        public void OrientWeaponsTowards(Vector3 lookPosition)
        {
            for (int i = 0; i < weapons.Length; i++)
            {
                Vector3 weaponForward = (lookPosition - weapons[i].WeaponRoot.transform.position).normalized;
                weapons[i].transform.forward = weaponForward;
            }
        }

        public bool TryAtack(Vector3 enemyPosition)
        {
            if (gameFlowManager.GameIsEnding)
                return false;

            OrientWeaponsTowards(enemyPosition);

            if ((lastTimeWeaponSwapped + delayAfterWeaponSwap) >= Time.time)
                return false;

            bool didFire = GetCurrentWeapon().HandleShootInputs(false, true, false);

            if (didFire && onAttack != null)
            {
                onAttack.Invoke();

                if (swapToNextWeapon && weapons.Length > 1)
                {
                    int nextWeaponIndex = (currentWeaponIndex + 1) % weapons.Length;
                    SetCurrentWeapon(nextWeaponIndex);
                }
            }

            return didFire;
        }

        public bool TryDropItem()
        {
            if (dropRate == 0 || lootPrefab == null)
                return false;
            else if (dropRate == 1)
                return true;
            else
                return (Random.value <= dropRate);
        }

        void FindAndInitializeAllWeapons()
        {
            if (weapons == null)
            {
                weapons = GetComponentsInChildren<WeaponController>();
                DebugUtility.HandleErrorIfNoComponentFound<WeaponController, EnemyController>(weapons.Length, this, gameObject);

                for (int i = 0; i < weapons.Length; i++)
                {
                    weapons[i].Owner = gameObject;
                }
            }
        }

        public WeaponController GetCurrentWeapon()
        {
            FindAndInitializeAllWeapons();
            if (currentWeapon == null)
            {
                SetCurrentWeapon(0);
            }

            DebugUtility.HandleErrorIfNullGetComponent<WeaponController, EnemyController>(currentWeapon, this, gameObject);

            return currentWeapon;
        }

        void SetCurrentWeapon(int index)
        {
            currentWeaponIndex = index;
            currentWeapon = weapons[currentWeaponIndex];
            if (swapToNextWeapon)
            {
                lastTimeWeaponSwapped = Time.time;
            }
            else
            {
                lastTimeWeaponSwapped = Mathf.NegativeInfinity;
            }
        }
    }
}