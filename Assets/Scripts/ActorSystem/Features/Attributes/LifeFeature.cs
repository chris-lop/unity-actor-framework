using UnityEngine;

public sealed class LifeFeature : MonoBehaviour, IActorFeature
{
    AttributesFeature _attrs;
    ActorContext _ctx;
    bool _isDyingOrDead;

    void Awake()
    {
        _attrs = GetComponent<AttributesFeature>();
    }

    public void Initialize(ActorContext ctx)
    {
        _ctx = ctx;
        _ctx.Events.OnDamage += OnDamage;
        _ctx.Events.OnDeathRequested += OnDeathRequested;
    }

    void OnDamage(DamageEvent e)
    {
        if (_isDyingOrDead)
            return;
        _attrs.ApplyDamage(e.Amount);
    }

    void OnDeathRequested()
    {
        if (_isDyingOrDead)
            return;
        _isDyingOrDead = true;
    }

    public void Tick(float dt) { }

    public void FixedTick(float fdt) { }

    public void Shutdown()
    {
        _ctx.Events.OnDamage -= OnDamage;
        _ctx.Events.OnDeathRequested -= OnDeathRequested;
    }
}
