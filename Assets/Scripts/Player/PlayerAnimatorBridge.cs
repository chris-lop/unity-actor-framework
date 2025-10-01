using UnityEngine;
using LastDescent.Gameplay.Animation;

namespace LastDescent.Player
{
    /// <summary>Thin layer to talk to Animator or flip sprite.</summary>
    public class PlayerAnimatorBridge : MonoBehaviour, IHurtDieAnimBridge
    {
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer sprite;

        static readonly int MoveSpeedHash = Animator.StringToHash("MoveSpeed");
        static readonly int AttackTriggerHash = Animator.StringToHash("AttackTrigger");
        static readonly int HurtTriggerHash = Animator.StringToHash("HurtTrigger");
        static readonly int DieTriggerHash = Animator.StringToHash("DieTrigger");


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
            if (animator) animator.SetTrigger(AttackTriggerHash);
        }
    }
}
