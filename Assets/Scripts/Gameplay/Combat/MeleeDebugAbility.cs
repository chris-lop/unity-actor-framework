using UnityEngine;
using LastDescent.Player;

namespace LastDescent.Gameplay.Combat
{
    public class MeleeDebugAbility : MonoBehaviour
    {
        [Header("Melee Debug")]
        [SerializeField, Min(0f)] private float range = 1.75f;
        [SerializeField, Min(0f)] private float damage = 1f;
        [SerializeField, Min(0f)] private float cooldownSeconds = 0.25f;
        [SerializeField, Range(1f, 360f)] private float arcDegrees = 180f;
        [SerializeField] private LayerMask damageableMask = ~0;

        private LifeState _selfLife;
        private float _nextReady;
        private Vector2 _lastFacing = Vector2.right;

        private void Awake()
        {
            if (!TryGetComponent(out _selfLife))
                _selfLife = GetComponentInParent<LifeState>();
        }

        /// <summary>Attempts to perform the melee attack. Returns true if activated.</summary>
        public bool TryActivate(Vector2 facing, PlayerAnimatorBridge animator = null)
        {
            if (Time.time < _nextReady) return false;

            // Normalize facing; if zero, default right
            if (facing.sqrMagnitude < 0.0001f) facing = Vector2.right;
            facing.Normalize();
            _lastFacing = facing;

            // Precompute cosine threshold for arc culling
            float halfArc = Mathf.Clamp(arcDegrees * 0.5f, 0.5f, 180f);
            float cosThreshold = Mathf.Cos(halfArc * Mathf.Deg2Rad); // keep targets within +/- halfArc

            var hits = Physics2D.OverlapCircleAll(transform.position, range);
            Vector2 origin = transform.position;

            for (int i = 0; i < hits.Length; i++)
            {
                var col = hits[i];
                if (!col) continue;

                // Layer mask filter
                if ((damageableMask.value & (1 << col.gameObject.layer)) == 0) continue;

                // Direction to target & arc filter
                Vector2 toTarget = (Vector2)col.transform.position - origin;
                float sqrMag = toTarget.sqrMagnitude;
                if (sqrMag < 0.0001f) continue; // overlapping self center; skip

                toTarget *= 1f / Mathf.Sqrt(sqrMag);
                float dot = Vector2.Dot(facing, toTarget);
                if (dot < cosThreshold) continue; // outside arc

                // LifeState & self filter
                if (col.TryGetComponent(out LifeState target) && !ReferenceEquals(target, _selfLife))
                {
                    target.Damage(damage, source: this);
                }
            }

            animator?.PlayAttack(facing);

            _nextReady = Time.time + cooldownSeconds;
            return true;
        }

      #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;

            Vector3 origin = transform.position;
            Vector3 fwd = Application.isPlaying && _lastFacing.sqrMagnitude > 0.0001f
                ? ((Vector3)_lastFacing).normalized
                : Vector3.right;

            if (arcDegrees <= 0f || range <= 0f) return;

            float halfArcDeg = Mathf.Clamp(arcDegrees * 0.5f, 0.5f, 180f);
            int segments = Mathf.Max(8, Mathf.RoundToInt(arcDegrees / 7.5f)); // ~24 for 180°
            float stepDeg = arcDegrees / segments;

            // Left edge of the arc relative to facing
            Quaternion baseRot = Quaternion.AngleAxis(-halfArcDeg, Vector3.forward);
            Vector3 edgeDir = baseRot * fwd;

            Vector3 prevPoint = origin + edgeDir * range;

            // Draw first radial
            Gizmos.DrawLine(origin, prevPoint);

            for (int i = 1; i <= segments; i++)
            {
                float angleDeg = -halfArcDeg + i * stepDeg;
                Vector3 dir = Quaternion.AngleAxis(angleDeg, Vector3.forward) * fwd;
                Vector3 nextPoint = origin + dir * range;

                Gizmos.DrawLine(prevPoint, nextPoint); // arc segment
                Gizmos.DrawLine(origin, nextPoint);    // radial
                prevPoint = nextPoint;
            }
        }
      #endif
    }
}
