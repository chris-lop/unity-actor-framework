using System;
using UnityEngine;

namespace LastDescent.Gameplay.Attributes
{
    /// <summary>
    /// Per-entity runtime attributes. Stores Base and Current values.
    /// v1 has no modifiers: Current = clamped(Base or Health).
    /// </summary>
    public class AttributeSet : MonoBehaviour
    {
        // Arrays indexed by (int)AttributeId — fast & simple to serialize.
        [SerializeField] private float[] _baseValues;
        [SerializeField] private float[] _currentValues;

        public event Action<AttributeId, float, float> OnBaseChanged;
        public event Action<AttributeId, float, float> OnCurrentChanged;

        const int AttributeCount = 5; // keep in sync with AttributeId

        void Awake()
        {
            EnsureArrays();
            // If Health uninitialized but MaxHealth set, clamp Health up to MaxHealth on spawn in Initializer.
            ClampAll();
        }

        void OnValidate()
        {
            EnsureArrays();
            ClampAll();
        }

        void EnsureArrays()
        {
            if (_baseValues == null || _baseValues.Length != AttributeCount)
                _baseValues = new float[AttributeCount];
            if (_currentValues == null || _currentValues.Length != AttributeCount)
                _currentValues = new float[AttributeCount];
        }

        void ClampAll()
        {
            // Generic non-negative clamp
            for (int i = 0; i < AttributeCount; i++)
            {
                if (_baseValues[i] < 0f) _baseValues[i] = 0f;
                if (_currentValues[i] < 0f) _currentValues[i] = 0f;
            }
            // Health special rule: 0..MaxHealth
            float maxHp = GetBase(AttributeId.MaxHealth);
            SetCurrent(AttributeId.Health, Mathf.Clamp(GetCurrent(AttributeId.Health), 0f, Mathf.Max(1f, maxHp)));
            // MaxHealth minimum of 1
            if (GetBase(AttributeId.MaxHealth) < 1f) SetBase(AttributeId.MaxHealth, 1f);
        }

        public float GetBase(AttributeId id) => _baseValues[(int)id];

        public float GetCurrent(AttributeId id)
        {
            // No modifiers yet; return current for Health, base for others if current is 0 by default.
            if (id == AttributeId.Health)
                return _currentValues[(int)id];
            // For non-Health, Current mirrors Base in v1 unless set explicitly.
            float cur = _currentValues[(int)id];
            return cur > 0f ? cur : AttributeCalculator.Final(_baseValues[(int)id]);
        }

        /// <summary>Sets Base and (optionally) snaps Current for non-Health.</summary>
        public void SetBase(AttributeId id, float value, bool alsoSetCurrent = true)
        {
            value = Mathf.Max(0f, value);
            int idx = (int)id;
            float old = _baseValues[idx];
            if (Mathf.Approximately(old, value)) return;

            _baseValues[idx] = value;
            OnBaseChanged?.Invoke(id, old, value);

            if (id == AttributeId.MaxHealth)
            {
                // Ensure MaxHealth >= 1, clamp current Health to new max (no free heal above cap)
                if (_baseValues[idx] < 1f)
                {
                    _baseValues[idx] = 1f;
                    OnBaseChanged?.Invoke(id, value, 1f);
                }
                float hp = Mathf.Min(GetCurrent(AttributeId.Health), _baseValues[idx]);
                SetCurrent(AttributeId.Health, hp);
            }
            else if (id != AttributeId.Health && alsoSetCurrent)
            {
                // Mirror to current for convenience (non-Health)
                SetCurrent(id, AttributeCalculator.Final(value));
            }
        }

        public void SetCurrent(AttributeId id, float value)
        {
            value = Mathf.Max(0f, value);
            if (id == AttributeId.Health)
            {
                float maxHp = Mathf.Max(1f, GetBase(AttributeId.MaxHealth));
                value = Mathf.Clamp(value, 0f, maxHp);
            }

            int idx = (int)id;
            float old = _currentValues[idx];
            if (Mathf.Approximately(old, value)) return;

            _currentValues[idx] = value;
            OnCurrentChanged?.Invoke(id, old, value);
        }

        /// <summary>Adds delta to Current (use negative for damage).</summary>
        public void ApplyDelta(AttributeId id, float delta)
        {
            SetCurrent(id, GetCurrent(id) + delta);
        }

        /// <summary>Convenience: read AttackSpeed safely even if unset.</summary>
        public float GetFinal(AttributeId id) => GetCurrent(id);
    }
}
