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
        public Transform detectionSourcePoint;
        public float detectionRange = 20f;
        public float attackRange = 10f;
        public float knownTargetTimeout = 4f;

        [Header("Animator")]
        public Animator animator;

        [Header("Events")]
        public UnityAction onDetectedTarget;
        public UnityAction onLostTarget;

        public GameObject knownDetectedTarget { get; private set; }
        public bool isTargetInAttackRange { get; private set; }
        public bool isSeeingTarget { get; private set; }
        public bool hadKnownTarget { get; private set; }

        private float timeLastSeenTarget = Mathf.NegativeInfinity;
        private ActorsManager actorsManager;

        private const string k_AnimAttackParameter = "Attack";
        private const string k_AnimOnDamagedParameter = "OnDamaged";

        protected virtual void Start()
        {
            // Initialize ActorsManager
            actorsManager = FindObjectOfType<ActorsManager>();
            DebugUtility.HandleErrorIfNullFindObject<ActorsManager, DetectionModule>(actorsManager, this);
        }

        public virtual void HandleTargetDetection(Actor actor, Collider[] selfColliders)
        {
            // Handle known target detection timeout
            if (knownDetectedTarget && !isSeeingTarget && (Time.time - timeLastSeenTarget) > knownTargetTimeout)
            {
                knownDetectedTarget = null;
            }

            // Find the closest visible hostile actor
            float sqrDetectionRange = detectionRange * detectionRange;
            isSeeingTarget = false;
            float closestSqrDistance = Mathf.Infinity;

            foreach (Actor otherActor in actorsManager.Actors)
            {
                if (otherActor.Affiliation != actor.Affiliation)
                {
                    float sqrDistance = (otherActor.transform.position - detectionSourcePoint.position).sqrMagnitude;
                    if (sqrDistance < sqrDetectionRange && sqrDistance < closestSqrDistance)
                    {
                        // Check for obstructions
                        RaycastHit[] hits = Physics.RaycastAll(detectionSourcePoint.position,
                            (otherActor.AimPoint.position - detectionSourcePoint.position).normalized, detectionRange,
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
                                isSeeingTarget = true;
                                closestSqrDistance = sqrDistance;

                                timeLastSeenTarget = Time.time;
                                knownDetectedTarget = otherActor.AimPoint.gameObject;
                            }
                        }
                    }
                }
            }

            isTargetInAttackRange = knownDetectedTarget != null &&
                                    Vector3.Distance(transform.position, knownDetectedTarget.transform.position) <=
                                    attackRange;

            // Detection events
            if (!hadKnownTarget && knownDetectedTarget != null)
            {
                OnDetect();
            }

            if (hadKnownTarget && knownDetectedTarget == null)
            {
                OnLostTarget();
            }

            // Remember if we already knew a target (for next frame)
            hadKnownTarget = knownDetectedTarget != null;
        }

        public virtual void OnLostTarget() => onLostTarget?.Invoke();

        public virtual void OnDetect() => onDetectedTarget?.Invoke();

        public virtual void OnDamaged(GameObject damageSource)
        {
            timeLastSeenTarget = Time.time;
            knownDetectedTarget = damageSource;

            if (animator)
            {
                animator.SetTrigger(k_AnimOnDamagedParameter);
            }
        }

        public virtual void OnAttack()
        {
            if (animator)
            {
                animator.SetTrigger(k_AnimAttackParameter);
            }
        }
    }
}