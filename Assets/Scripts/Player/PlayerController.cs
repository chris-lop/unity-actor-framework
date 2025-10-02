using UnityEngine;
using LastDescent.Input;
using LastDescent.Gameplay.Combat;

namespace LastDescent.Player
{
    /// <summary>Reads PlayerCommand and decides what the character should do.</summary>
    [RequireComponent(typeof(CharacterMotor2D))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private ScriptableObjects.PlayerTuning tuning;
        [SerializeField] private PlayerAnimatorBridge animatorBridge;
        [SerializeField] private LifeState lifeState;
        private IPlayerInputSource _input;
        private CharacterMotor2D _motor;

        public void SetInputSource(IPlayerInputSource source) => _input = source;

        private void Awake()
        {
            _motor = GetComponent<CharacterMotor2D>();
            if (lifeState == null) lifeState = GetComponent<LifeState>();
        }

        private void Update()
        {
            if (_input == null || tuning == null || _motor == null) return;

            var cmd = _input.ReadCommand();

            // Aim facing (sprite/art can read this via AnimatorBridge)
            Vector2 facing = (cmd.aimWorld - (Vector2)transform.position);
            if (facing.sqrMagnitude > 0.0001f)
                animatorBridge?.FaceDirection(facing.normalized);

            // Desired velocity
            Vector2 desired = cmd.move.normalized * tuning.moveSpeed;
            _motor.SetDesiredVelocity(desired);

            // DEBUG ability: attack press -> take 1 damage through attributes
            if (cmd.attackPressed)
            {
                lifeState?.Damage(1f, source: this);
                animatorBridge?.PlayAttack(facing.normalized);
            }

            // Anim
            animatorBridge?.SetMoveSpeed(_motor.CurrentSpeed);
        }
    }
}
