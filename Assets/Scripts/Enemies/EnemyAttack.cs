using UnityEngine;
using LastDescent.Gameplay.Combat;

namespace LastDescent.Enemies
{
    /// <summary>
    /// Hit detection and timed damage while AI is in Attack state.
    /// Finds LifeState targets on configured layers and calls Damage().
    /// </summary>
    public class EnemyAttack : MonoBehaviour
    {
        [SerializeField] private EnemyConfig config;
        [SerializeField] private Transform attackOrigin; // optional pivot; defaults to self
        public System.Action OnSwing;
        float _cooldownTimer;
        bool _active;

        // Called by AI when entering Attack
        public void Begin(EnemyConfig cfgFromAI)
        {
            if (cfgFromAI != null) config = cfgFromAI;
            _active = true;
            _cooldownTimer = 0f; // swing immediately on entry
        }

        // Called by AI when exiting Attack
        public void End()
        {
            _active = false;
        }

        void Update()
        {
            if (!_active || config == null) return;

            _cooldownTimer -= Time.deltaTime;
            if (_cooldownTimer > 0f) return;

            Vector3 origin = attackOrigin ? attackOrigin.position : transform.position;
            var hits = Physics2D.OverlapCircleAll(origin, config.attackRange, config.targetLayers);

            foreach (var h in hits)
            {
                if (h == null) continue;

                var life = h.GetComponentInParent<LifeState>() ?? h.GetComponent<LifeState>();
                if (life == null || life.IsDead) continue;

                life.Damage(config.contactDamagePerHit, gameObject);
                OnSwing?.Invoke();
                _cooldownTimer = config.attackCooldown;
                break; // one target per swing
            }
        }

        void OnDrawGizmosSelected()
        {
            if (config == null) return;
            Vector3 origin = attackOrigin ? attackOrigin.position : transform.position;
            Gizmos.DrawWireSphere(origin, config.attackRange);
        }
    }
}
