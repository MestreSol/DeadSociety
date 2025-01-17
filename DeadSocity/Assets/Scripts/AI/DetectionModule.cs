using System.Linq;
using Core.Managers;
using Core.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace AI
{
    public class DetectionModule : MonoBehaviour
    {
        [Header("Detection Settings")]
        public Transform DetectionSourcePoint;
        public float DetectionRange = 20f;
        public float AttackRange = 10f;
        public float KnownTargetTimeout = 4f;

        [Header("Animator")]
        public Animator Animator;

        [Header("Events")]
        public UnityAction onDetectedTarget;
        public UnityAction onLostTarget;

        public GameObject KnownDetectedTarget { get; private set; }
        public bool IsTargetInAttackRange { get; private set; }
        public bool IsSeeingTarget { get; private set; }
        public bool HadKnownTarget { get; private set; }

        private float TimeLastSeenTarget = Mathf.NegativeInfinity;
        private ActorsManager m_ActorsManager;

        private const string k_AnimAttackParameter = "Attack";
        private const string k_AnimOnDamagedParameter = "OnDamaged";

        protected virtual void Start()
        {
            // Initialize ActorsManager
            m_ActorsManager = FindObjectOfType<ActorsManager>();
            DebugUtility.HandleErrorIfNullFindObject<ActorsManager, DetectionModule>(m_ActorsManager, this);
        }

        public virtual void HandleTargetDetection(Actor actor, Collider[] selfColliders)
        {
            // Handle known target detection timeout
            if (KnownDetectedTarget && !IsSeeingTarget && (Time.time - TimeLastSeenTarget) > KnownTargetTimeout)
            {
                KnownDetectedTarget = null;
            }

            // Find the closest visible hostile actor
            float sqrDetectionRange = DetectionRange * DetectionRange;
            IsSeeingTarget = false;
            float closestSqrDistance = Mathf.Infinity;

            foreach (Actor otherActor in m_ActorsManager.Actors)
            {
                if (otherActor.Affiliation != actor.Affiliation)
                {
                    float sqrDistance = (otherActor.transform.position - DetectionSourcePoint.position).sqrMagnitude;
                    if (sqrDistance < sqrDetectionRange && sqrDistance < closestSqrDistance)
                    {
                        // Check for obstructions
                        RaycastHit[] hits = Physics.RaycastAll(DetectionSourcePoint.position,
                            (otherActor.AimPoint.position - DetectionSourcePoint.position).normalized, DetectionRange,
                            -1, QueryTriggerInteraction.Ignore);
                        RaycastHit closestValidHit = new RaycastHit { distance = Mathf.Infinity };
                        bool foundValidHit = false;

                        foreach (var hit in hits)
                        {
                            if (!selfColliders.Contains(hit.collider) && hit.distance < closestValidHit.distance)
                            {
                                closestValidHit = hit;
                                foundValidHit = true;
                            }
                        }

                        if (foundValidHit)
                        {
                            Actor hitActor = closestValidHit.collider.GetComponentInParent<Actor>();
                            if (hitActor == otherActor)
                            {
                                IsSeeingTarget = true;
                                closestSqrDistance = sqrDistance;

                                TimeLastSeenTarget = Time.time;
                                KnownDetectedTarget = otherActor.AimPoint.gameObject;
                            }
                        }
                    }
                }
            }

            IsTargetInAttackRange = KnownDetectedTarget != null &&
                                    Vector3.Distance(transform.position, KnownDetectedTarget.transform.position) <=
                                    AttackRange;

            // Detection events
            if (!HadKnownTarget && KnownDetectedTarget != null)
            {
                OnDetect();
            }

            if (HadKnownTarget && KnownDetectedTarget == null)
            {
                OnLostTarget();
            }

            // Remember if we already knew a target (for next frame)
            HadKnownTarget = KnownDetectedTarget != null;
        }

        public virtual void OnLostTarget() => onLostTarget?.Invoke();

        public virtual void OnDetect() => onDetectedTarget?.Invoke();

        public virtual void OnDamaged(GameObject damageSource)
        {
            TimeLastSeenTarget = Time.time;
            KnownDetectedTarget = damageSource;

            if (Animator)
            {
                Animator.SetTrigger(k_AnimOnDamagedParameter);
            }
        }

        public virtual void OnAttack()
        {
            if (Animator)
            {
                Animator.SetTrigger(k_AnimAttackParameter);
            }
        }
    }
}