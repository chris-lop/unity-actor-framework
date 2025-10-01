using UnityEngine;
using LastDescent.Gameplay.Combat;

namespace LastDescent.Gameplay.Animation
{
    /// <summary>
    /// Reusable binder: listens to LifeState and drives any IHurtDieAnimBridge.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(LifeState))]
    public class LifeToAnimatorBridge : MonoBehaviour
    {
        [Tooltip("Any component on this object or its children that implements IHurtDieAnimBridge.")]
        public MonoBehaviour BridgeComponent; // must implement IHurtDieAnimBridge

        private IHurtDieAnimBridge _bridge;
        private LifeState _life;

        [SerializeField] float _hurtCooldown = 0.08f;
        private float _lastHurtTime;

        void Reset()
        {
            _life = GetComponent<LifeState>();
            if (BridgeComponent == null)
                BridgeComponent = GetComponentInChildren<MonoBehaviour>(true);
        }

        void Awake()
        {
            if (_life == null) _life = GetComponent<LifeState>();

            // If not manually assigned, try to find any IHurtDieAnimBridge in children.
            if (BridgeComponent == null)
                BridgeComponent = FindBridgeInChildren();

            _bridge = BridgeComponent as IHurtDieAnimBridge;

            if (_bridge == null)
                Debug.LogWarning($"{name}: LifeToAnimatorBridge missing IHurtDieAnimBridge.");
        }

        void OnEnable()
        {
            if (_life == null) _life = GetComponent<LifeState>();
            if (_life != null)
            {
                _life.OnDamaged += HandleDamaged;
                _life.OnDied += HandleDied;
                _life.OnRevived += HandleRevived;
            }
        }

        void OnDisable()
        {
            if (_life != null)
            {
                _life.OnDamaged -= HandleDamaged;
                _life.OnDied -= HandleDied;
                _life.OnRevived -= HandleRevived;
            }
        }

        void HandleDamaged(float amount, object source)
        {
            if (_bridge == null || amount <= 0f) return;
            if (Time.time - _lastHurtTime < _hurtCooldown) return;
            _lastHurtTime = Time.time;
            _bridge.PlayHurt();
        }

        void HandleDied()
        {
            _bridge?.PlayDie();
        }

        void HandleRevived()
        {
            // Optional only if your bridge implements it
            try { _bridge?.PlayRevive(); } catch { /* default no-op */ }
        }

        MonoBehaviour FindBridgeInChildren()
        {
            // Scan children for the first component that implements the interface
            foreach (var mb in GetComponentsInChildren<MonoBehaviour>(true))
                if (mb is IHurtDieAnimBridge) return mb;
            return null;
        }
    }
}
