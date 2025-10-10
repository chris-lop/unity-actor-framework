
using UnityEngine;

public sealed class LifeFeature : MonoBehaviour, IActorFeature {
    AttributesFeature _attrs; ActorContext _ctx; bool _dead;
    void Awake(){ _attrs = GetComponent<AttributesFeature>(); }
    public void Initialize(ActorContext ctx){ _ctx = ctx; _ctx.Events.OnDamage += OnDamage; _ctx.Events.OnDeath += OnDeath; }
    void OnDamage(DamageEvent e){ if (_dead) return; _attrs.ApplyDamage(e.Amount); }
    void OnDeath(){ if (_dead) return; _dead = true; gameObject.SetActive(false); }
    public void Tick(float dt){} public void FixedTick(float fdt){} public void Shutdown(){ _ctx.Events.OnDamage -= OnDamage; _ctx.Events.OnDeath -= OnDeath; }
}