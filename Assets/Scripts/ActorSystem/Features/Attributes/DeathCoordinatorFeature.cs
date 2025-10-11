using UnityEngine;

public sealed class DeathCoordinatorFeature : MonoBehaviour, IActorFeature
{
    ActorContext _ctx;
    bool _isDying;

    public void FixedTick(float fdt) { }

    public void Initialize(ActorContext ctx)
    {
        _ctx = ctx;
        _ctx.Events.OnDeathRequested += OnDeathRequested;
        _ctx.Events.OnDeathFinished += OnDeathFinished;
    }

    public void Shutdown()
    {
        if (_ctx != null)
        {
            _ctx.Events.OnDeathRequested -= OnDeathRequested;
            _ctx.Events.OnDeathFinished -= OnDeathFinished;
        }
    }

    public void Tick(float dt) { }

    private void OnDeathRequested()
    {
        if (_isDying)
            return;
        _isDying = true;

        // freeze gameplay systems
        var proc = GetComponent<ActorCommandProcessor>();
        if (proc)
            proc.enabled = false;
        var motor = GetComponent<Motor2DFeature>();
        if (motor)
            motor.enabled = false;
        var ability = GetComponent<AbilityRunnerFeature>();
        if (ability)
            ability.enabled = false;
        var col = GetComponent<Collider2D>();
        if (col)
            col.enabled = false;
        var rb = GetComponent<Rigidbody2D>();
        if (rb)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        _ctx.Events.RaiseDeathStarted();
    }

    private void OnDeathFinished()
    {
        // TODO: Handle pooled objects, for now just destroy
        Destroy(gameObject);
    }
}
