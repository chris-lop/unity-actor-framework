using UnityEngine;

public sealed class AnimationPresenter : MonoBehaviour, IActorFeature {
    [SerializeField] Animator animator; ActorContext _ctx;
    public void Initialize(ActorContext ctx){
        _ctx = ctx;
        if (!animator) animator = GetComponentInChildren<Animator>();
        _ctx.Events.OnAbilityCast += e => animator?.SetTrigger("Cast");
        _ctx.Events.OnDeath += () => animator?.SetTrigger("Die");
    }
    public void Tick(float dt){} public void FixedTick(float fdt){} public void Shutdown(){}
}