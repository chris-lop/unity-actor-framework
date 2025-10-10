using UnityEngine;

public sealed class AttributesFeature : MonoBehaviour, IActorFeature {
    public float Health { get; private set; }
    public float Damage { get; private set; }
    ActorContext _ctx;
    public void Initialize(ActorContext ctx){
        _ctx = ctx;
        var a = ctx.Definition.AttributeSet;
        Health = a.BaseHealth; Damage = a.BaseDamage;
    }
    public void ApplyDamage(float amount){
        Health -= amount;
        if (Health <= 0) _ctx.Events.RaiseDeath();
    }
    public void Tick(float dt){} public void FixedTick(float fdt){} public void Shutdown(){}
}