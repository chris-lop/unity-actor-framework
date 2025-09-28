using UnityEngine;
using UnityEngine.InputSystem;
using LastDescent.Input;

namespace LastDescent.Player
{
    /// <summary>Bridges Unity Input System to PlayerCommand.</summary>
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputAdapter : MonoBehaviour, IPlayerInputSource
    {
        [Header("Actions (from .inputactions)")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference aimAction;


        [Header("Aim")]
        [SerializeField] private Camera worldCamera;

        private Vector2 _cachedAim;

        private void Awake()
        {
            if (worldCamera == null) worldCamera = Camera.main;
        }

        private void OnEnable()
        {
            moveAction?.action.Enable();
            aimAction?.action.Enable();
        }

        private void OnDisable()
        {
            moveAction?.action.Disable();
            aimAction?.action.Disable();
        }

        public PlayerCommand ReadCommand()
        {
            var cmd = PlayerCommand.Empty;

            // 1) Move
            cmd.move = moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;

            // 2) Aim
            // If mouse is present, convert screen to world.
            if (Mouse.current != null && worldCamera != null)
            {
                Vector2 screen = Mouse.current.position.ReadValue();
                Vector3 world = worldCamera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, Mathf.Abs(worldCamera.transform.position.z)));
                cmd.aimWorld = new Vector2(world.x, world.y);
                _cachedAim = cmd.aimWorld;
            }
            else
            {
                // Fallback for gamepad right-stick providing a direction vector; project to world in front of player.
                Vector2 aimDir = aimAction != null ? aimAction.action.ReadValue<Vector2>() : Vector2.zero;
                if (aimDir.sqrMagnitude > 0.001f)
                {
                    // Put an arbitrary distance ahead in world space; the controller will only use the direction anyway.
                    var pos = transform.position;
                    cmd.aimWorld = pos + (Vector3)(aimDir.normalized * 5f);
                    _cachedAim = cmd.aimWorld;
                }
                else
                {
                    cmd.aimWorld = _cachedAim; // keep last good aim target
                }
            }

            return cmd;
        }
    }
}