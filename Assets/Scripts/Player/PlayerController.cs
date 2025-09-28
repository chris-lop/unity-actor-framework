using UnityEngine;
using LastDescent.Input;

namespace LastDescent.Player
{
    /// <summary>Reads PlayerCommand and decides what the character should do.</summary>
    [RequireComponent(typeof(CharacterMotor2D))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private ScriptableObjects.PlayerTuning tuning;
        [SerializeField] private PlayerAnimatorBridge animatorBridge;

        private IPlayerInputSource _input;
        private CharacterMotor2D _motor;

        public void SetInputSource(IPlayerInputSource source) => _input = source;

        private void Awake()
        {
            _motor = GetComponent<CharacterMotor2D>();
        }

        private void Update()
        {
            if (_input == null) return;

            var cmd = _input.ReadCommand();

            // Aim facing (sprite/art can read this via AnimatorBridge)
            Vector2 facing = (cmd.aimWorld - (Vector2)transform.position);
            if (facing.sqrMagnitude > 0.0001f)
                animatorBridge?.FaceDirection(facing.normalized);

            // Desired velocity
            Vector2 desired = cmd.move.normalized * tuning.moveSpeed;
            _motor.SetDesiredVelocity(desired);

            // Anim
            animatorBridge?.SetMoveSpeed(_motor.CurrentSpeed);
        }
    }
}
