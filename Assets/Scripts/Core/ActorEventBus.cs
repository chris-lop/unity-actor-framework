using System;

[Serializable] public class ActorEventBus {
    public event Action<DamageEvent> OnDamage;
    public event Action<AbilityCastEvent> OnAbilityCast;
    public event Action OnDeath;
    public void Raise(DamageEvent e) => OnDamage?.Invoke(e);
    public void Raise(AbilityCastEvent e) => OnAbilityCast?.Invoke(e);
    public void RaiseDeath() => OnDeath?.Invoke();
}
public struct DamageEvent { public int SourceId; public float Amount; }
public struct AbilityCastEvent { public string AbilityId; }