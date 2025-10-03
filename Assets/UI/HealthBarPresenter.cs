using LastDescent.Gameplay.Combat;
using UnityEngine;

namespace LastDescent.UI
{
    /// <summary>
    /// Per-entity glue: registers a ProgressBar-based health bar for this entity with the overlay.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HealthBarPresenter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private LifeState life;
        [SerializeField] private Transform anchor;

        [Header("Behavior")]
        [SerializeField, Tooltip("Always show for this entity (e.g., bosses).")]
        private bool alwaysVisible = false;

        [SerializeField, Tooltip("Seconds to keep the bar visible after a change when not always visible.")]
        private float lingerSeconds = 2.0f;

        public LifeState Life => life;
        public Transform Anchor => anchor ? anchor : transform;
        public bool AlwaysVisible => alwaysVisible;
        public float LingerSeconds => lingerSeconds;

        private void Reset()
        {
            if (!anchor)
            {
                var t = transform.Find("HealthBarAnchor");
                anchor = t ? t : transform;
            }
        }

        private void OnEnable()
        {
            if (life == null)
            {
                Debug.LogError($"{nameof(HealthBarPresenter)} on {name} is missing a {nameof(LifeState)} reference.");
                enabled = false;
                return;
            }

            // Try now; if service isn’t ready, wait for it.
            if (HealthBarOverlayService.IsReady && HealthBarOverlayService.Instance != null)
            {
                HealthBarOverlayService.Instance.Register(this);
            }
            else
            {
                HealthBarOverlayService.OnReady += RegisterWhenReady;
            }
        }

        private void OnDisable()
        {
            HealthBarOverlayService.OnReady -= RegisterWhenReady;
            HealthBarOverlayService.Instance?.Unregister(this);
        }

        private void RegisterWhenReady()
        {
            // Guard against double-register if OnEnable already succeeded
            if (HealthBarOverlayService.Instance != null)
                HealthBarOverlayService.Instance.Register(this);
        }
    }
}
