using System;
using UnityEngine;
using LastDescent.Gameplay.Attributes;

namespace LastDescent.Gameplay.Combat
{
    /// <summary>
    /// Simple health wrapper + life/death events.
    /// </summary>
    public class LifeState : MonoBehaviour
    {
        [SerializeField] private AttributeSet _attributes;

        public event Action<float, object> OnDamaged; // amount, source
        public event Action<float, object> OnHealed;  // amount, source
        public event Action OnDied;
        public event Action OnRevived;

        bool _isDead;

        void Reset()
        {
            _attributes = GetComponent<AttributeSet>();
        }

        void Awake()
        {
            if (_attributes == null) _attributes = GetComponent<AttributeSet>();
            if (_attributes == null)
            {
                Debug.LogWarning($"{name}: LifeState missing AttributeSet.");
                return;
            }

            _isDead = _attributes.GetCurrent(AttributeId.Health) <= 0f;
            _attributes.OnCurrentChanged += HandleAttrChanged;
        }

        void OnDestroy()
        {
            if (_attributes != null)
                _attributes.OnCurrentChanged -= HandleAttrChanged;
        }

        void HandleAttrChanged(AttributeId id, float oldV, float newV)
        {
            if (id != AttributeId.Health) return;

            // Damage / Heal callbacks
            float delta = newV - oldV;
            if (delta < 0f) OnDamaged?.Invoke(-delta, null);
            else if (delta > 0f) OnHealed?.Invoke(delta, null);

            // Death / Revive transitions
            if (!_isDead && newV <= 0f)
            {
                _isDead = true;
                OnDied?.Invoke();
            }
            else if (_isDead && newV > 0f)
            {
                _isDead = false;
                OnRevived?.Invoke();
            }
        }

        public void Damage(float amount, object source = null)
        {
            if (amount <= 0f || _attributes == null) return;
            _attributes.ApplyDelta(AttributeId.Health, -amount);
        }

        public void Heal(float amount, object source = null)
        {
            if (amount <= 0f || _attributes == null) return;
            _attributes.ApplyDelta(AttributeId.Health, amount);
        }

        public bool IsDead => _isDead;
        public float Health => _attributes.GetCurrent(AttributeId.Health);
        public float MaxHealth => _attributes.GetBase(AttributeId.MaxHealth);
    }
}
