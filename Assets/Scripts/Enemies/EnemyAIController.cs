using System.Collections;
using UnityEngine;
using LastDescent.Gameplay.Combat;

namespace LastDescent.Enemies
{
    /// <summary>
    /// Simple 2D top-down enemy brain:
    /// - Picks a target (Portal by tag, else nearest LifeState on targetLayers within aggro).
    /// - Moves via Rigidbody2D toward target.
    /// - Switches to Attack when within attackRange.
    /// - Subscribes to own LifeState events to handle death/cleanup.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(LifeState))]
    public class EnemyAIController : MonoBehaviour
    {
        [SerializeField] private EnemyConfig config;
        [Tooltip("Optional explicit target. If null, looks for tag 'Portal' on spawn, then nearest LifeState.")]
        [SerializeField] private Transform explicitTarget;
        [Tooltip("Animator bridge with the same API as PlayerAnimatorBridge.")]
        [SerializeField] private EnemyAnimatorBridge animBridge;
        [Header("Movement")]
        [SerializeField, Min(0f), Tooltip("Extra stop distance to avoid shoving into big colliders.")]
        private float stoppingBuffer = 0.1f;
        [Header("Targeting")]
        [SerializeField] private string portalTag = "Portal";
        private Transform _portal;
        private Rigidbody2D _rb;
        private LifeState _life;
        private EnemyAttack _attack;
        private Transform _target;

        private enum AIState { Chase, Attack, Dead }
        private AIState _state;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            _life = GetComponent<LifeState>();
            _attack = GetComponent<EnemyAttack>();
            if (_attack == null) _attack = gameObject.AddComponent<EnemyAttack>();
            if (!animBridge) animBridge = GetComponentInChildren<EnemyAnimatorBridge>();
        }

        void OnEnable()
        {
            _life.OnDied += HandleDied;
            _life.OnRevived += HandleRevived;

            _target = explicitTarget ? explicitTarget : FindDefaultTarget();
            _state = _life.IsDead ? AIState.Dead : AIState.Chase;
            if (animBridge)
            {
                // ensure idle visuals on spawn unless dead
                animBridge.SetMoveSpeed(0f);
                if (_state == AIState.Dead) animBridge.PlayDie();
            }
        }

        void OnDisable()
        {
            if (_life != null)
            {
                _life.OnDied -= HandleDied;
                _life.OnRevived -= HandleRevived;
            }

            if (_attack != null)
                _attack.OnSwing -= HandleAttackSwing;
        }

        void Update()
        {
            if (_state == AIState.Dead) return;

            // Lost or dead target? reacquire
            if (_target == null || TargetIsDead(_target))
                _target = FindDefaultTarget();

            // Dynamically switch between player (in aggro) and portal (fallback)
            MaybeSwapTargetBasedOnAggro();

            switch (_state)
            {
                case AIState.Chase:
                    TickChase();
                    break;
                case AIState.Attack:
                    TickAttack();
                    break;
            }
        }

        void TickChase()
        {
            if (_target == null)
            {
                _rb.linearVelocity = Vector2.zero;
                if (animBridge) animBridge.SetMoveSpeed(0f);
                return;
            }

            GetDirAndDistanceToTarget(_target, out var dir, out var dist);

            if (animBridge && dir.sqrMagnitude > 0.0001f)
            {
                var n = dir.normalized;
                animBridge.FaceDirection(n);
            }

            if (dist <= (config.attackRange + stoppingBuffer))
            {
                _state = AIState.Attack;
                _attack.Begin(config);
                _attack.OnSwing -= HandleAttackSwing;
                _attack.OnSwing += HandleAttackSwing;
                if (animBridge) animBridge.SetMoveSpeed(0f);
                return;
            }

            if (IsPortal(_target))
            {
                // Always path to portal, even if it's far beyond aggro.
                Vector2 vel = dir.normalized * config.moveSpeed;
                _rb.linearVelocity = vel;
                if (animBridge) animBridge.SetMoveSpeed(vel.magnitude);
            }
            else
            {
                // Non-portal targets (e.g., player) only chased within aggro.
                if (dist <= config.aggroRange)
                {
                    Vector2 vel = dir.normalized * config.moveSpeed;
                    _rb.linearVelocity = vel;
                    if (animBridge) animBridge.SetMoveSpeed(vel.magnitude);
                }
                else
                {
                    _rb.linearVelocity = Vector2.zero;
                    if (animBridge) animBridge.SetMoveSpeed(0f);
                }
            }
        }

        void TickAttack()
        {
            if (_target == null)
            {
                _state = AIState.Chase;
                _attack.End();
                _attack.OnSwing -= HandleAttackSwing;
                if (animBridge) animBridge.SetMoveSpeed(_rb.linearVelocity.magnitude);
                return;
            }

            GetDirAndDistanceToTarget(_target, out _, out var dist);
            if (dist > (config.attackRange + stoppingBuffer) * 1.1f) // hysteresis
            {
                _state = AIState.Chase;
                _attack.End();
                _attack.OnSwing -= HandleAttackSwing;
                if (animBridge) animBridge.SetMoveSpeed(_rb.linearVelocity.magnitude);
                return;
            }

            GetDirAndDistanceToTarget(_target, out var look, out _);
            if (look.sqrMagnitude > 0.001f)
            {
                if (animBridge) animBridge.FaceDirection(look);
            }
        }

        void HandleDied()
        {
            _state = AIState.Dead;
            _rb.linearVelocity = Vector2.zero;
            _attack.OnSwing -= HandleAttackSwing;
            var colls = GetComponentsInChildren<Collider2D>();
            foreach (var c in colls) c.enabled = false;
            if (animBridge) animBridge.PlayDie();
            if (animBridge) animBridge.SetMoveSpeed(0f);
            StartCoroutine(DespawnAfter(config != null ? config.despawnDelay : 2f));
        }

        void HandleRevived()
        {
            var colls = GetComponentsInChildren<Collider2D>();
            foreach (var c in colls) c.enabled = true;
            _state = AIState.Chase;
            if (animBridge) animBridge.SetMoveSpeed(0f);
        }

        IEnumerator DespawnAfter(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            Destroy(gameObject);
        }

        Transform FindDefaultTarget()
        {
            // First try to find any valid LifeState within aggro (typically the player).
            var playerInAggro = FindNearestLifeStateWithinAggro();
            if (playerInAggro != null) return playerInAggro;

            // Otherwise go for the portal (even if far).
            var p = GetPortal();
            return p != null ? p : null;
        }

        bool TargetIsDead(Transform t)
        {
            if (t == null) return true;
            var ls = t.GetComponentInParent<LifeState>() ?? t.GetComponent<LifeState>();
            return ls == null || ls.IsDead;
        }

        void HandleAttackSwing()
        {
            if (!animBridge) return;
            Vector2 dir = _target
                ? ((Vector2)_target.position - (Vector2)transform.position).normalized
                : (_rb.linearVelocity.sqrMagnitude > 0.0001f ? _rb.linearVelocity.normalized : Vector2.right);
            animBridge.PlayAttack(dir);
        }

        bool IsPortal(Transform t) => t != null && _portal != null && t == _portal;

        Transform GetPortal()
        {
            if (_portal && _portal.gameObject.activeInHierarchy) return _portal;
            var p = GameObject.FindGameObjectWithTag(portalTag);
            _portal = p ? p.transform : null;
            return _portal;
        }

        Transform FindNearestLifeStateWithinAggro()
        {
            var hits = Physics2D.OverlapCircleAll(transform.position, config.aggroRange, config.targetLayers);
            float best = float.PositiveInfinity;
            Transform bestT = null;
            foreach (var h in hits)
            {
                if (!h) continue;
                var ls = h.GetComponentInParent<LifeState>() ?? h.GetComponent<LifeState>();
                if (ls == null || ls.IsDead) continue;

                float d = Vector2.Distance(transform.position, h.transform.position);
                if (d < best)
                {
                    best = d;
                    bestT = h.transform;
                }
            }
            return bestT;
        }

        void MaybeSwapTargetBasedOnAggro()
        {
            // If we're on the portal and the player re-enters aggro, switch back to player.
            if (IsPortal(_target))
            {
                var playerInAggro = FindNearestLifeStateWithinAggro();
                if (playerInAggro != null) _target = playerInAggro;
                return;
            }

            // If our current (non-portal) target is outside aggro, chase the portal instead.
            if (_target != null)
            {
                float dist = Vector2.Distance(transform.position, _target.position);
                if (dist > config.aggroRange)
                {
                    var p = GetPortal();
                    if (p != null) _target = p;
                }
            }
        }
        
        // Returns direction from enemy to the target's collider surface (or transform if no collider),
        // and distance to that point.
        void GetDirAndDistanceToTarget(Transform t, out Vector2 dir, out float dist)
        {
            Vector2 from = transform.position;
            Vector2 to = GetClosestPointOnTarget(t, from);
            Vector2 v = to - from;
            dist = v.magnitude;
            dir = dist > 0.0001f ? v / dist : Vector2.zero;
        }

        Vector2 GetClosestPointOnTarget(Transform t, Vector2 from)
        {
            if (!t) return (Vector2)transform.position;
            // Try collider on target or its parent
            var col = t.GetComponentInParent<Collider2D>() ?? t.GetComponent<Collider2D>();
            if (col && col.enabled)
            {
                return col.OverlapPoint(from) ? from : col.ClosestPoint(from);
            }
            return (Vector2)t.position;
        }
    }
}
