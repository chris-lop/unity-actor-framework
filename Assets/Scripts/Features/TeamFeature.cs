using UnityEngine;

public sealed class TeamFeature : MonoBehaviour, IActorFeature {
    public int Team => _team; int _team; ActorContext _ctx;
    public void Initialize(ActorContext ctx){ _ctx = ctx; _team = ctx.Definition.Team; }
    public void Tick(float dt){} public void FixedTick(float fdt){} public void Shutdown(){}
}