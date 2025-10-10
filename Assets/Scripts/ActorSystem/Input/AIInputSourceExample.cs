using UnityEngine;

namespace LastDescent.Input
{
    /// <summary>
    /// Example AI input source implementation - demonstrates how to create custom input sources.
    /// This is a simple example that can be expanded for actual AI behavior.
    /// 
    /// Usage: Attach to an actor GameObject alongside CommandProcessorFeature&lt;ActorCommand&gt;
    /// </summary>
    public class AIInputSourceExample : MonoBehaviour, IInputSource<ActorCommand>
    {
        [Header("AI Behavior Settings")]
        [SerializeField] private float detectionRange = 10f;
        [SerializeField] private float attackRange = 3f;
        [SerializeField] private LayerMask targetMask;

        private Transform _target;
        private float _nextAttackTime;

        public ActorCommand ReadCommand()
        {
            var cmd = ActorCommand.Empty;

            // Find target if we don't have one
            if (_target == null)
            {
                FindTarget();
            }

            // If we have a target, generate commands to pursue and attack it
            if (_target != null)
            {
                Vector2 toTarget = _target.position - transform.position;
                float distanceToTarget = toTarget.magnitude;

                // Set aim at target
                cmd.aimWorld = _target.position;

                // Move towards target if too far
                if (distanceToTarget > attackRange)
                {
                    cmd.move = toTarget.normalized;
                }
                else
                {
                    cmd.move = Vector2.zero; // Stop when in range
                }

                // Attack if in range and cooldown is ready
                if (distanceToTarget <= attackRange && Time.time >= _nextAttackTime)
                {
                    cmd.attackPressed = true;
                    _nextAttackTime = Time.time + 1f; // Simple cooldown
                }
            }

            return cmd;
        }

        private void FindTarget()
        {
            // Simple detection: find closest target in range
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRange, targetMask);
            
            float closestDist = float.MaxValue;
            Transform closest = null;

            foreach (var hit in hits)
            {
                if (hit.transform == transform) continue; // Skip self

                float dist = Vector2.Distance(transform.position, hit.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = hit.transform;
                }
            }

            _target = closest;
        }

        private void OnDrawGizmosSelected()
        {
            // Visualize detection and attack ranges
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}


