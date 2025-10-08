using UnityEngine;
using LastDescent.Gameplay.Animation;
using LastDescent.Gameplay.Combat;

namespace LastDescent.Enemies
{
    /// <summary>Thin layer to talk to Animator or flip sprite (same contract as PlayerAnimatorBridge).</summary>
    public class EnemyAnimatorBridge : MonoBehaviour, IHurtDieAnimBridge
    {
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer sprite;
        private LifeState life;

        static readonly int MoveSpeedHash    = Animator.StringToHash("MoveSpeed");
        static readonly int AttackTriggerHash = Animator.StringToHash("AttackTrigger");
        static readonly int HurtTriggerHash   = Animator.StringToHash("HurtTrigger");
        static readonly int DieTriggerHash    = Animator.StringToHash("DieTrigger");

        void Reset()
        {
            if (!animator) animator = GetComponentInChildren<Animator>();
            if (!sprite)   sprite   = GetComponentInChildren<SpriteRenderer>();
            if (!life)     life     = GetComponentInParent<LifeState>();
        }

        void OnEnable()
        {
            if (!life) life = GetComponentInParent<LifeState>();

            if (life)
            {
                life.OnDamaged += HandleDamaged;
                life.OnDied    += HandleDied;
                life.OnRevived += HandleRevived;
            }
            else
            {
                Debug.LogWarning($"{name}: EnemyAnimatorBridge has no LifeState assigned.");
            }
        }

        void OnDisable()
        {
            if (life)
            {
                life.OnDamaged -= HandleDamaged;
                life.OnDied    -= HandleDied;
                life.OnRevived -= HandleRevived;
            }
        }

        // ===== LifeState event handlers =====
        void HandleDamaged(float amount, object source) => PlayHurt();
        void HandleDied() => PlayDie();
        void HandleRevived() { /* optional: reset state */ }

        // ===== IHurtDieAnimBridge-compatible API =====
        public void SetMoveSpeed(float speed)
        {
            if (animator) animator.SetFloat(MoveSpeedHash, speed);
        }

        public void FaceDirection(Vector2 dir)
        {
            if (sprite && Mathf.Abs(dir.x) > 0.01f)
                sprite.flipX = dir.x < 0f;
        }

        public void PlayHurt()
        {
            if (animator) animator.SetTrigger(HurtTriggerHash);
        }

        public void PlayDie()
        {
            if (animator) animator.SetTrigger(DieTriggerHash);
        }

        public void PlayAttack(Vector2 dir)
        {
            // We also face in the attack direction for consistency.
            FaceDirection(dir);
            if (animator) animator.SetTrigger(AttackTriggerHash);
        }
    }
}
