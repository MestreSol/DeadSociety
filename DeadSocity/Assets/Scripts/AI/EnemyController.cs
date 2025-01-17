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
        public Renderer Renderer;
        public int MaterialIndex;

        public RendererIndexData(Renderer renderer, int index)
        {
            Renderer = renderer;
            MaterialIndex = index;
        }
    }

    public float SelfDestructYHeight = -20f;

    public float PathReachingRadius = 2f;
    
        public float OrientationSpeed = 10f;

    public float DeathDuration = 0f;


    public bool SwapToNextWeapon = false;

    public float DelayAfterWeaponSwap = 0f;

    public Material EyeColorMaterial;

    public Color DefaultEyeColor;

    public Color AttackEyeColor;

    public Material BodyMaterial;

    public Gradient OnHitBodyGradient;

    public float FlashOnHitDuration = 0.5f;

    public AudioClip DamageTick;

    public GameObject DeathVfx;

    public Transform DeathVfxSpawnPoint;

    public GameObject LootPrefab;

    public float DropRate = 1f;

    public Color PathReachingRangeColor = Color.yellow;

    public Color AttackRangeColor = Color.red;

    public Color DetectionRangeColor = Color.blue;

    public UnityAction onAttack;
    public UnityAction onDetectedTarget;
    public UnityAction onLostTarget;
    public UnityAction onDamaged;

    List<RendererIndexData> m_BodyRenderers = new List<RendererIndexData>();
    MaterialPropertyBlock m_BodyFlashMaterialPropertyBlock;
    float m_LastTimeDamaged = float.NegativeInfinity;

    RendererIndexData m_EyeRendererData;
    MaterialPropertyBlock m_EyeColorMaterialPropertyBlock;

    public PatrolPath PatrolPath { get; set; }
    public GameObject KnownDetectedTarget => DetectionModule.KnownDetectedTarget;
    public bool IsTargetInAttackRange => DetectionModule.IsTargetInAttackRange;
    public bool IsSeeingTarget => DetectionModule.IsSeeingTarget;
    public bool HadKnownTarget => DetectionModule.HadKnownTarget;
    public NavMeshAgent NavMeshAgent { get; private set; }
    public DetectionModule DetectionModule { get; private set; }

    int m_PathDestinationNodeIndex;
    EnemyManager m_EnemyManager;
    ActorsManager m_ActorsManager;
    Health m_Health;
    Actor m_Actor;
    Collider[] m_SelfColliders;
    GameFlowManager m_GameFlowManager;
    bool m_WasDamagedThisFrame;
    float m_LastTimeWeaponSwapped = Mathf.NegativeInfinity;
    int m_CurrentWeaponIndex;
    WeaponController m_CurrentWeapon;
    WeaponController[] m_Weapons;
    NavigationModule m_NavigationModule;

    void Start()
    {
        m_EnemyManager = FindObjectOfType<EnemyManager>();
        DebugUtility.HandleErrorIfNullFindObject<EnemyManager, EnemyController>(m_EnemyManager, this);

        m_ActorsManager = FindObjectOfType<ActorsManager>();
        DebugUtility.HandleErrorIfNullFindObject<ActorsManager, EnemyController>(m_ActorsManager, this);

        m_EnemyManager.RegisterEnemy(this);

        m_Health = GetComponent<Health>();
        DebugUtility.HandleErrorIfNullGetComponent<Health, EnemyController>(m_Health, this, gameObject);

        m_Actor = GetComponent<Actor>();
        DebugUtility.HandleErrorIfNullGetComponent<Actor, EnemyController>(m_Actor, this, gameObject);

        NavMeshAgent = GetComponent<NavMeshAgent>();
        m_SelfColliders = GetComponentsInChildren<Collider>();

        m_GameFlowManager = FindObjectOfType<GameFlowManager>();
        DebugUtility.HandleErrorIfNullFindObject<GameFlowManager, EnemyController>(m_GameFlowManager, this);

        m_Health.OnDie += OnDie;
        m_Health.OnDamaged += OnDamaged;

        FindAndInitializeAllWeapons();
        var weapon = GetCurrentWeapon();
        weapon.ShowWeapon(true);

        var detectionModules = GetComponentsInChildren<DetectionModule>();
        DebugUtility.HandleErrorIfNoComponentFound<DetectionModule, EnemyController>(detectionModules.Length, this,
            gameObject);
        DebugUtility.HandleWarningIfDuplicateObjects<DetectionModule, EnemyController>(detectionModules.Length,
            this, gameObject);
        DetectionModule = detectionModules[0];
        DetectionModule.onDetectedTarget += OnDetectedTarget;
        DetectionModule.onLostTarget += OnLostTarget;
        onAttack += DetectionModule.OnAttack;

        var navigationModules = GetComponentsInChildren<NavigationModule>();
        DebugUtility.HandleWarningIfDuplicateObjects<DetectionModule, EnemyController>(detectionModules.Length,
            this, gameObject);
        if (navigationModules.Length > 0)
        {
            m_NavigationModule = navigationModules[0];
            NavMeshAgent.speed = m_NavigationModule.MoveSpeed;
            NavMeshAgent.angularSpeed = m_NavigationModule.AngularSpeed;
            NavMeshAgent.acceleration = m_NavigationModule.Acceleration;
        }

        foreach (var renderer in GetComponentsInChildren<Renderer>(true))
        {
            for (int i = 0; i < renderer.sharedMaterials.Length; i++)
            {
                if (renderer.sharedMaterials[i] == EyeColorMaterial)
                {
                    m_EyeRendererData = new RendererIndexData(renderer, i);
                }

                if (renderer.sharedMaterials[i] == BodyMaterial)
                {
                    m_BodyRenderers.Add(new RendererIndexData(renderer, i));
                }
            }
        }

        m_BodyFlashMaterialPropertyBlock = new MaterialPropertyBlock();

        if (m_EyeRendererData.Renderer != null)
        {
            m_EyeColorMaterialPropertyBlock = new MaterialPropertyBlock();
            m_EyeColorMaterialPropertyBlock.SetColor("_EmissionColor", DefaultEyeColor);
            m_EyeRendererData.Renderer.SetPropertyBlock(m_EyeColorMaterialPropertyBlock,
                m_EyeRendererData.MaterialIndex);
        }
    }

    void Update()
    {
        EnsureIsWithinLevelBounds();

        DetectionModule.HandleTargetDetection(m_Actor, m_SelfColliders);

        Color currentColor = OnHitBodyGradient.Evaluate((Time.time - m_LastTimeDamaged) / FlashOnHitDuration);
        m_BodyFlashMaterialPropertyBlock.SetColor("_EmissionColor", currentColor);
        foreach (var data in m_BodyRenderers)
        {
            data.Renderer.SetPropertyBlock(m_BodyFlashMaterialPropertyBlock, data.MaterialIndex);
        }

        m_WasDamagedThisFrame = false;
    }

    void EnsureIsWithinLevelBounds()
    {
        if (transform.position.y < SelfDestructYHeight)
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnLostTarget()
    {
        onLostTarget.Invoke();

        if (m_EyeRendererData.Renderer != null)
        {
            m_EyeColorMaterialPropertyBlock.SetColor("_EmissionColor", DefaultEyeColor);
            m_EyeRendererData.Renderer.SetPropertyBlock(m_EyeColorMaterialPropertyBlock,
                m_EyeRendererData.MaterialIndex);
        }
    }

    void OnDetectedTarget()
    {
        onDetectedTarget.Invoke();

        if (m_EyeRendererData.Renderer != null)
        {
            m_EyeColorMaterialPropertyBlock.SetColor("_EmissionColor", AttackEyeColor);
            m_EyeRendererData.Renderer.SetPropertyBlock(m_EyeColorMaterialPropertyBlock,
                m_EyeRendererData.MaterialIndex);
        }
    }

    public void OrientTowards(Vector3 lookPosition)
    {
        Vector3 lookDirection = Vector3.ProjectOnPlane(lookPosition - transform.position, Vector3.up).normalized;
        if (lookDirection.sqrMagnitude != 0f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation =
                Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * OrientationSpeed);
        }
    }

    bool IsPathValid()
    {
        return PatrolPath && PatrolPath.PathNodes.Count > 0;
    }

    public void ResetPathDestination()
    {
        m_PathDestinationNodeIndex = 0;
    }

    public void SetPathDestinationToClosestNode()
    {
        if (IsPathValid())
        {
            int closestPathNodeIndex = 0;
            for (int i = 0; i < PatrolPath.PathNodes.Count; i++)
            {
                float distanceToPathNode = PatrolPath.GetDistanceToNode(transform.position, i);
                if (distanceToPathNode < PatrolPath.GetDistanceToNode(transform.position, closestPathNodeIndex))
                {
                    closestPathNodeIndex = i;
                }
            }

            m_PathDestinationNodeIndex = closestPathNodeIndex;
        }
        else
        {
            m_PathDestinationNodeIndex = 0;
        }
    }

    public Vector3 GetDestinationOnPath()
    {
        if (IsPathValid())
        {
            return PatrolPath.GetPositionOfPathNode(m_PathDestinationNodeIndex);
        }
        else
        {
            return transform.position;
        }
    }

    public void SetNavDestination(Vector3 destination)
    {
        if (NavMeshAgent)
        {
            NavMeshAgent.SetDestination(destination);
        }
    }

    public void UpdatePathDestination(bool inverseOrder = false)
    {
        if (IsPathValid())
        {
            if ((transform.position - GetDestinationOnPath()).magnitude <= PathReachingRadius)
            {
                m_PathDestinationNodeIndex =
                    inverseOrder ? (m_PathDestinationNodeIndex - 1) : (m_PathDestinationNodeIndex + 1);
                if (m_PathDestinationNodeIndex < 0)
                {
                    m_PathDestinationNodeIndex += PatrolPath.PathNodes.Count;
                }

                if (m_PathDestinationNodeIndex >= PatrolPath.PathNodes.Count)
                {
                    m_PathDestinationNodeIndex -= PatrolPath.PathNodes.Count;
                }
            }
        }
    }

    void OnDamaged(float damage, GameObject damageSource)
    {
        if (damageSource && !damageSource.GetComponent<EnemyController>())
        {
            // pursue the player
            DetectionModule.OnDamaged(damageSource);
            
            onDamaged?.Invoke();
            m_LastTimeDamaged = Time.time;
        
            if (DamageTick && !m_WasDamagedThisFrame)
                AudioUtility.CreateSFX(DamageTick, transform.position, AudioUtility.AudioGroups.DamageTick, 0f);
        
            m_WasDamagedThisFrame = true;
        }
    }

    void OnDie()
    {
        var vfx = Instantiate(DeathVfx, DeathVfxSpawnPoint.position, Quaternion.identity);
        Destroy(vfx, 5f);

        m_EnemyManager.UnregisterEnemy(this);

        if (TryDropItem())
        {
            Instantiate(LootPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject, DeathDuration);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = PathReachingRangeColor;
        Gizmos.DrawWireSphere(transform.position, PathReachingRadius);

        if (DetectionModule != null)
        {
            Gizmos.color = DetectionRangeColor;
            Gizmos.DrawWireSphere(transform.position, DetectionModule.DetectionRange);

            Gizmos.color = AttackRangeColor;
            Gizmos.DrawWireSphere(transform.position, DetectionModule.AttackRange);
        }
    }

    public void OrientWeaponsTowards(Vector3 lookPosition)
    {
        for (int i = 0; i < m_Weapons.Length; i++)
        {
            Vector3 weaponForward = (lookPosition - m_Weapons[i].WeaponRoot.transform.position).normalized;
            m_Weapons[i].transform.forward = weaponForward;
        }
    }

    public bool TryAtack(Vector3 enemyPosition)
    {
        if (m_GameFlowManager.GameIsEnding)
            return false;

        OrientWeaponsTowards(enemyPosition);

        if ((m_LastTimeWeaponSwapped + DelayAfterWeaponSwap) >= Time.time)
            return false;

        bool didFire = GetCurrentWeapon().HandleShootInputs(false, true, false);

        if (didFire && onAttack != null)
        {
            onAttack.Invoke();

            if (SwapToNextWeapon && m_Weapons.Length > 1)
            {
                int nextWeaponIndex = (m_CurrentWeaponIndex + 1) % m_Weapons.Length;
                SetCurrentWeapon(nextWeaponIndex);
            }
        }

        return didFire;
    }

    public bool TryDropItem()
    {
        if (DropRate == 0 || LootPrefab == null)
            return false;
        else if (DropRate == 1)
            return true;
        else
            return (Random.value <= DropRate);
    }

    void FindAndInitializeAllWeapons()
    {
        if (m_Weapons == null)
        {
            m_Weapons = GetComponentsInChildren<WeaponController>();
            DebugUtility.HandleErrorIfNoComponentFound<WeaponController, EnemyController>(m_Weapons.Length, this,
                gameObject);

            for (int i = 0; i < m_Weapons.Length; i++)
            {
                m_Weapons[i].Owner = gameObject;
            }
        }
    }

    public WeaponController GetCurrentWeapon()
    {
        FindAndInitializeAllWeapons();
        if (m_CurrentWeapon == null)
        {
            SetCurrentWeapon(0);
        }

        DebugUtility.HandleErrorIfNullGetComponent<WeaponController, EnemyController>(m_CurrentWeapon, this,
            gameObject);

        return m_CurrentWeapon;
    }

    void SetCurrentWeapon(int index)
    {
        m_CurrentWeaponIndex = index;
        m_CurrentWeapon = m_Weapons[m_CurrentWeaponIndex];
        if (SwapToNextWeapon)
        {
            m_LastTimeWeaponSwapped = Time.time;
        }
        else
        {
            m_LastTimeWeaponSwapped = Mathf.NegativeInfinity;
        }
    }
}
}