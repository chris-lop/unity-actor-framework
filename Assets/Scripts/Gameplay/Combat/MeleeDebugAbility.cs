using UnityEngine;
using LastDescent.Player;
using System.Collections.Generic;

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
        [Header("Temporary: Active Hit Window")]
        [SerializeField, Tooltip("If true, we keep sampling during a short window so late targets can get hit.")]
        private bool useActiveWindow = true;
        [SerializeField, Min(0f), Tooltip("Delay before the window starts (lets the animation wind-up).")]
        private float preImpactDelay = 0.05f;
        [SerializeField, Min(0f), Tooltip("How long to keep sampling for hits after activation.")]
        private float activeWindowSeconds = 0.15f;
        private LifeState _selfLife;
        private float _nextReady;
        private Vector2 _lastFacing = Vector2.right;
        private readonly HashSet<LifeState> _hitThisWindow = new HashSet<LifeState>();
        private Coroutine _swingRoutine;

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

            if (useActiveWindow)
            {
                if (_swingRoutine != null) StopCoroutine(_swingRoutine);
                _swingRoutine = StartCoroutine(SwingWindow(facing));
            }
            else
            {
                // Original single-sample behavior
                SampleOnce(facing, null);
            }

            animator?.PlayAttack(facing);

            _nextReady = Time.time + cooldownSeconds;
            return true;
        }

        // Samples once and applies damage; dedupes per-window if a set is provided.
        private void SampleOnce(Vector2 facing, HashSet<LifeState> hitOnce = null)
        {
            // Normalize facing; if zero, default right
            if (facing.sqrMagnitude < 0.0001f) facing = Vector2.right;
            facing.Normalize();

            float halfArc = Mathf.Clamp(arcDegrees * 0.5f, 0.5f, 180f);
            float cosThreshold = Mathf.Cos(halfArc * Mathf.Deg2Rad);

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
                if (sqrMag < 0.0001f) continue;

                toTarget *= 1f / Mathf.Sqrt(sqrMag);
                float dot = Vector2.Dot(facing, toTarget);
                if (dot < cosThreshold) continue;

                // LifeState & self filter
                if (col.TryGetComponent(out LifeState target) && !ReferenceEquals(target, _selfLife))
                {
                    if (hitOnce != null && hitOnce.Contains(target)) continue;
                    target.Damage(damage, source: this);
                    hitOnce?.Add(target);
                }
            }
        }
        
        private System.Collections.IEnumerator SwingWindow(Vector2 facing)
        {
            // Optional wind-up delay to better align with animation impact
            if (preImpactDelay > 0f)
                yield return new WaitForSeconds(preImpactDelay);

            _hitThisWindow.Clear();
            float endTime = Time.time + activeWindowSeconds;
            while (Time.time < endTime)
            {
                // Sample every frame so targets that move into range during the swing get hit once
                SampleOnce(facing, _hitThisWindow);
                yield return null;
            }
            _hitThisWindow.Clear();
            _swingRoutine = null;
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
