using UnityEngine;

public sealed class TeamFeature : MonoBehaviour, IActorFeature
{
    public int Team => _team;

    int _team;
    ActorContext _ctx;

    public void Initialize(ActorContext ctx)
    {
        if (ctx == null) { Debug.LogError("TeamFeature.Initialize: ctx is null", this); enabled = false; return; }
        if (ctx.Definition == null) { Debug.LogError("TeamFeature.Initialize: ctx.Definition is null", this); enabled = false; return; }

        _ctx  = ctx;
        _team = ctx.Definition.Team;
    }

    public void Tick(float dt) { }
    public void FixedTick(float fdt) { }
    public void Shutdown() { }
}
