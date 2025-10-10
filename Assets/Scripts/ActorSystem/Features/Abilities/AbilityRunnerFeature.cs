using UnityEngine;

[DisallowMultipleComponent]
public sealed class AbilityRunnerFeature : MonoBehaviour, IActorFeature
{
    [Header("Raycast")]
    [SerializeField] private Transform hitOrigin;
    [SerializeField] private LayerMask hurtboxMask;
    [SerializeField] private float startOffset = 0.2f;
    [SerializeField] private float maxDistance = 10f;

    private ActorContext _ctx;
    private TeamFeature _team;
    private float _cd;

    public void Initialize(ActorContext ctx)
    {
        _ctx = ctx;
        _team = GetComponent<TeamFeature>();
        if (!hitOrigin) hitOrigin = transform;
    }

    public bool TryCast(AbilityDefinition def, Vector2 dir)
    {
        if (def == null || _cd > 0f) return false;

        dir = dir.sqrMagnitude > 0.0001f ? dir.normalized : (Vector2)transform.right;

        // push origin slightly forward so we don’t start inside our own collider
        Vector2 origin = (Vector2)hitOrigin.position + dir * startOffset;
        Debug.DrawLine(hitOrigin.position, origin, Color.green, 0.1f);

        // visual debug
        Debug.DrawRay(origin, dir * maxDistance, Color.red, 0.2f);

        // ---- The simple, correct overload ----
        RaycastHit2D hit = Physics2D.Raycast(origin, dir, maxDistance, hurtboxMask);

        if (hit.collider == null) return false;

        // fetch the target actor
        var targetKernel = hit.collider.GetComponentInParent<ActorKernel>();
        if (targetKernel == null) return false;

        // ignore self
        if (targetKernel == _ctx.Kernel) return false;

        // ignore same team (no friendly fire)
        var targetTeam = targetKernel.GetComponent<TeamFeature>();
        if (_team != null && targetTeam != null && _team.Team == targetTeam.Team) return false;

        // apply damage to the *target's* bus
        targetKernel.Ctx.Events.Raise(new DamageEvent {
            SourceId = _ctx.ActorId,
            Amount   = def.Damage
        });

        _ctx.Events.Raise(new AbilityCastEvent { AbilityId = def.Id });
        _cd = def.Cooldown;
        return true;
    }

    public void Tick(float dt){ if (_cd > 0f) _cd -= dt; }
    public void FixedTick(float fdt) {}
    public void Shutdown() {}
}