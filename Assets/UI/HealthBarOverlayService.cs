using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastDescent.UI
{
    /// <summary>
    /// Manages a single UI Toolkit overlay of ProgressBar health bars for many entities.
    /// Uses pooling, world->panel placement, and event-driven updates from LifeState.
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public sealed class HealthBarOverlayService : MonoBehaviour
    {
        [Header("UI Toolkit")]
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private VisualTreeAsset healthbarTemplate;
        [SerializeField] private PanelSettings panelSettings;

        [Header("Layout")]
        [SerializeField, Tooltip("Pixel offset above the anchor (Y+ is up in world).")]
        private Vector2 pixelOffset = new Vector2(0f, 24f);

        [SerializeField, Tooltip("Hide bars that are off-screen.")]
        private bool hideOffscreen = true;

        [Header("Pooling")]
        [SerializeField, Tooltip("Initial pooled instances.")]
        private int initialPool = 32;

        public static HealthBarOverlayService Instance { get; private set; }
        public static bool IsReady { get; private set; }
        public static event Action OnReady;

        private sealed class BarEntry
        {
            public HealthBarPresenter presenter;
            public VisualElement root;
            public ProgressBar bar;
            public float visibleUntil;
            public bool visible;
        }

        private readonly Stack<VisualElement> _pool = new Stack<VisualElement>(64);
        private readonly Dictionary<HealthBarPresenter, BarEntry> _active = new Dictionary<HealthBarPresenter, BarEntry>(128);

        private VisualElement _overlayRoot;
        private PanelSettings _panel;
        private Camera _camera;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"{nameof(HealthBarOverlayService)}: duplicate instance; destroying {name}.");
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (!uiDocument) uiDocument = GetComponent<UIDocument>();
            if (!uiDocument)
            {
                Debug.LogError($"{nameof(HealthBarOverlayService)} requires a {nameof(UIDocument)}.");
                enabled = false;
                return;
            }

            _overlayRoot = uiDocument.rootVisualElement;
            _overlayRoot.pickingMode = PickingMode.Ignore;
            _overlayRoot.style.flexGrow = 1f;
            _panel = panelSettings ? panelSettings : uiDocument.panelSettings;

            _camera = Camera.main;
            if (!_camera)
                Debug.LogWarning($"{nameof(HealthBarOverlayService)}: no Camera.main; call SetCamera() at runtime.");

            EnsurePool(initialPool);

            // Mark ready and notify any waiting presenters
            IsReady = true;
            OnReady?.Invoke();
            OnReady = null;
        }

        private void LateUpdate()
        {
            if (_active.Count == 0 || _overlayRoot == null || _panel == null || _camera == null)
                return;

            float now = Time.unscaledTime;

            foreach (var kv in _active)
            {
                var entry = kv.Value;
                var p = entry.presenter;

                // Visibility
                bool shouldShow = p.AlwaysVisible || now <= entry.visibleUntil;

                // Cull offscreen if configured
                Vector2 panelPos;
                bool onPanel = TryWorldToPanel(p.Anchor.position, out panelPos);

                if (hideOffscreen && !onPanel) shouldShow = false;

                if (!shouldShow)
                {
                    if (entry.visible)
                    {
                        entry.root.style.display = DisplayStyle.None;
                        entry.visible = false;
                    }
                    continue;
                }

                entry.root.style.position = Position.Absolute;
                entry.root.style.left = panelPos.x + pixelOffset.x;
                entry.root.style.top  = panelPos.y - pixelOffset.y;

                // Move the element so its center/bottom sits on (left,top)
                entry.root.style.translate = new Translate(
                    new Length(-50, LengthUnit.Percent),  // X: -50% (center horizontally)
                    new Length(-100, LengthUnit.Percent)  // Y: -100% (use bottom edge)
                );

                if (!entry.visible)
                {
                    entry.root.style.display = DisplayStyle.Flex;
                    entry.visible = true;
                }
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // --- Public API -------------------------------------------------------

        public void SetCamera(Camera cam) => _camera = cam;

        public void Register(HealthBarPresenter presenter)
        {
            if (presenter == null || presenter.Life == null || healthbarTemplate == null || _overlayRoot == null)
                return;

            if (_active.ContainsKey(presenter)) return;

            var ve = GetFromPool();
            var progress = ve.Q<ProgressBar>("HealthBar");
            if (progress == null)
            {
                Debug.LogError("HealthBarOverlayService: UXML must contain a ProgressBar named 'HealthBar'.");
                ReturnToPool(ve);
                return;
            }

            // Initialize bar range & value from LifeState
            var life = presenter.Life;
            progress.lowValue = 0f;
            progress.highValue = Mathf.Max(1f, life.MaxHealth);
            progress.value = Mathf.Clamp(life.CurrentHealth, progress.lowValue, progress.highValue);
            progress.title = $"{Mathf.CeilToInt(progress.value)}/{Mathf.CeilToInt(progress.highValue)}";

            var entry = new BarEntry
            {
                presenter = presenter,
                root = ve,
                bar = progress,
                visibleUntil = presenter.AlwaysVisible ? float.PositiveInfinity : 0f,
                visible = presenter.AlwaysVisible
            };

            // Subscribe to health changes
            life.OnHealthChanged += HandleHealthChanged;
            life.OnDied += HandleDied;

            void HandleHealthChanged(float current, float max)
            {
                if (!_active.ContainsKey(presenter)) return;

                if (!Mathf.Approximately(entry.bar.highValue, max))
                    entry.bar.highValue = Mathf.Max(1f, max);

                entry.bar.value = Mathf.Clamp(current, entry.bar.lowValue, entry.bar.highValue);
                entry.bar.title = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";

                if (!presenter.AlwaysVisible)
                    entry.visibleUntil = Time.unscaledTime + presenter.LingerSeconds;
            }

            void HandleDied()
            {
                Unregister(presenter);
            }

            // Record & show initial visibility state
            ve.style.display = presenter.AlwaysVisible ? DisplayStyle.Flex : DisplayStyle.None;

            _active.Add(presenter, entry);
        }

        public void Unregister(HealthBarPresenter presenter)
        {
            if (presenter == null) return;

            if (_active.TryGetValue(presenter, out var entry))
            {
                // Unsubscribe from LifeState events
                if (presenter.Life != null)
                {
                    presenter.Life.OnHealthChanged -= HandleHealthChanged;
                    presenter.Life.OnDied -= HandleDied;
                }

                ReturnToPool(entry.root);
                _active.Remove(presenter);
            }

            // Local method must match the one used in Register's += (method group).
            void HandleHealthChanged(float current, float max) { }
            void HandleDied() { }
        }

        // --- Internals --------------------------------------------------------

        private VisualElement GetFromPool()
        {
            if (_pool.Count == 0)
                EnsurePool(1);

            var ve = _pool.Pop();
            _overlayRoot.Add(ve);
            return ve;
        }

        private void ReturnToPool(VisualElement ve)
        {
            if (ve == null) return;
            ve.style.display = DisplayStyle.None;
            ve.RemoveFromHierarchy();
            _pool.Push(ve);
        }

        private void EnsurePool(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var ve = healthbarTemplate.Instantiate();
                ve.style.position = Position.Absolute;
                ve.pickingMode = PickingMode.Ignore;
                ve.style.display = DisplayStyle.None;
                _pool.Push(ve);
            }
        }

        private bool TryWorldToPanel(Vector3 worldPos, out Vector2 panelPos)
        {
          #if UNITY_2022_2_OR_NEWER
            var panel = _overlayRoot?.panel;
            if (panel != null)
            {
                panelPos = RuntimePanelUtils.CameraTransformWorldToPanel(panel, worldPos, _camera);
                if (float.IsNaN(panelPos.x) || float.IsNaN(panelPos.y)) return false;

                if (!hideOffscreen) return true;
                var rt = _overlayRoot.worldBound;
                return rt.Contains(panelPos);
            }
            panelPos = default;
            return false;
          #else
            // Fallback using ScreenPoint mapping
            var sp = _camera.WorldToScreenPoint(worldPos);
            if (sp.z < 0f) { panelPos = Vector2.zero; return false; }

            var rootBounds = _overlayRoot.worldBound;
            panelPos = new Vector2(sp.x, rootBounds.height - sp.y);

            if (!hideOffscreen) return true;
            return rootBounds.Contains(panelPos);
          #endif
        }
    }
}
