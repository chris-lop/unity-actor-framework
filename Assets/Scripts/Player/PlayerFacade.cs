using UnityEngine;
using LastDescent.Input;

namespace LastDescent.Player
{
    /// <summary>One-stop wiring hub on the player prefab.</summary>
    public class PlayerFacade : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private PlayerInputAdapter inputAdapter;
        [SerializeField] private PlayerController controller;
        [SerializeField] private CharacterMotor2D motor;
        [SerializeField] private PlayerHealth health;
        [SerializeField] private PlayerAnimatorBridge animatorBridge;
        [SerializeField] private PlayerAuthority authority;

        private void Reset()
        {
            inputAdapter = GetComponent<PlayerInputAdapter>();
            controller = GetComponent<PlayerController>();
            motor = GetComponent<CharacterMotor2D>();
            health = GetComponent<PlayerHealth>();
            animatorBridge = GetComponent<PlayerAnimatorBridge>();
            authority = GetComponent<PlayerAuthority>();
        }

        private void Awake()
        {
            // Wire input based on authority
            IPlayerInputSource source = (authority != null && authority.IsLocalAuthority && inputAdapter != null)
                ? (IPlayerInputSource)inputAdapter
                : NullInputSource.Instance;

            controller?.SetInputSource(source);
        }
    }
}
