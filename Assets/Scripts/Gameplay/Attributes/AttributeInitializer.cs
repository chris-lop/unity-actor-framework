using UnityEngine;

namespace LastDescent.Gameplay.Attributes
{
    /// <summary>
    /// Seeds Base values from Defaults then overrides with Profile.
    /// Sets Health = MaxHealth on spawn.
    /// </summary>
    public class AttributeInitializer : MonoBehaviour
    {
        [Tooltip("Optional defaults; if provided, uses their DefaultBaseValue first.")]
        public AttributeProfile Profile;

        [SerializeField] private AttributeSet _set;

        void Reset()
        {
            _set = GetComponent<AttributeSet>();
        }

        void Awake()
        {
            if (_set == null) _set = GetComponent<AttributeSet>();
            if (_set == null) return;

            // 1) Profile
            if (Profile != null)
            {
                foreach (var e in Profile.Entries)
                    _set.SetBase(e.Id, e.BaseValue);
            }

            // 2) Spawn at full health
            float maxHp = _set.GetBase(AttributeId.MaxHealth);
            _set.SetCurrent(AttributeId.Health, Mathf.Max(1f, maxHp));
        }
    }
}
