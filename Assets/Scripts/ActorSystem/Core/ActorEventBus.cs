using System;

[Serializable]
public class ActorEventBus
{
    public event Action<DamageEvent> OnDamage;
    public event Action<AbilityCastEvent> OnAbilityCast;

    // Death events
    public event Action OnDeathRequested;
    public event Action OnDeathStarted;
    public event Action OnDeathFinished;

    public void Raise(DamageEvent e) => OnDamage?.Invoke(e);

    public void Raise(AbilityCastEvent e) => OnAbilityCast?.Invoke(e);

    public void RaiseDeathRequested() => OnDeathRequested?.Invoke();

    public void RaiseDeathStarted() => OnDeathStarted?.Invoke();

    public void RaiseDeathFinished() => OnDeathFinished?.Invoke();
}

public struct DamageEvent
{
    public int SourceId;
    public float Amount;
}

public struct AbilityCastEvent
{
    public string AbilityId;
}
