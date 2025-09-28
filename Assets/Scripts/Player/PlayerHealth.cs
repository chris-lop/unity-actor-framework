using System;
using UnityEngine;

namespace LastDescent.Player
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private int maxHP = 5;
        public int CurrentHP { get; private set; }

        public event Action<int,int> OnDamaged; // (newHP, amount)
        public event Action OnDeath;

        private void Awake() => CurrentHP = maxHP;

        public void ApplyDamage(int amount)
        {
            if (CurrentHP <= 0) return;
            amount = Mathf.Max(0, amount);
            CurrentHP = Mathf.Max(0, CurrentHP - amount);
            OnDamaged?.Invoke(CurrentHP, amount);
            if (CurrentHP == 0) OnDeath?.Invoke();
        }
    }
}
