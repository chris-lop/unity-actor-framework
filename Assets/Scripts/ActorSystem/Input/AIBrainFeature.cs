using System.Linq;
using LastDescent.Input;
using UnityEngine;

[RequireComponent(typeof(AbilityRunnerFeature))]
public abstract class AIBrainFeature : MonoBehaviour, IActorFeature, IInputSource<ActorCommand>
{
    [Header("Targeting")]
    [SerializeField]
    protected LayerMask targetMask;

    [SerializeField]
    protected bool requireLineOfSight = false;

    [SerializeField]
    protected LayerMask losObstacles;

    [SerializeField]
    protected float reacquireInterval = 0.25f;

    protected ActorContext Ctx;
    protected AbilityRunnerFeature Runner;
    protected TeamFeature Team;
    protected Transform Target;

    private float _reacquireTimer;
    private float _lastTime;

    // ---------- lifecycle ----------
    public virtual void Initialize(ActorContext ctx)
    {
        Ctx = ctx;
        Runner = GetComponent<AbilityRunnerFeature>();
        Team = GetComponent<TeamFeature>();

        if (!Runner)
            Debug.LogWarning($"{name}: missing AbilityRunnerFeature.");

        _lastTime = Time.time;
        OnInitialized();
    }

    protected virtual void OnInitialized() { }

    public virtual void Tick(float dt) { }

    public virtual void FixedTick(float fdt) { }

    public virtual void Shutdown() { }

    // ---------- input source ----------
    public virtual ActorCommand ReadCommand()
    {
        var cmd = ActorCommand.Empty;
        if (Ctx == null)
            return cmd;

        // compute dt for timers
        float now = Time.time;
        float dt = now - _lastTime;
        _lastTime = now;

        // reacquire target on interval
        _reacquireTimer -= dt;
        if (Target == null || _reacquireTimer <= 0f || !IsTargetValid(Target))
        {
            Target = AcquireTarget();
            _reacquireTimer = reacquireInterval;
        }

        if (Target == null)
            return cmd;

        // aim
        cmd.aimWorld = Target.position;

        // distance / dir
        Vector2 toTarget = Target.position - transform.position;
        float distance = toTarget.magnitude;
        Vector2 dir = distance > 0.0001f ? toTarget / distance : (Vector2)transform.right;

        // select ability
        var pick = SelectAbility(distance);
        var ability = pick?.def;
        var slot = pick?.slot ?? -1;

        // let subclass decide movement & attack policy
        UpdateBehavior(ability, distance, dir, ref cmd);

        if (ability != null && slot >= 0 && ShouldCast(ability, slot, distance))
        {
            cmd.attackPressed = true;
            cmd.requestedAbilitySlot = slot;
        }

        return cmd;
    }

    // ---------- behavior hooks ----------
    /// <summary>
    /// Choose an ability given current distance.
    /// Default: shortest that reaches if ready; else longest to approach for.
    /// </summary>
    protected virtual (AbilityDefinition def, int slot)? SelectAbility(float distance)
    {
        var abilities = Ctx?.Definition?.Abilities;
        if (abilities == null || abilities.Length == 0)
            return null;

        var pairs = abilities
            .Select((ability, slot) => new { ability, slot })
            .Where(p => p.ability != null)
            .ToList();
        if (pairs.Count == 0)
            return null;

        var ready = pairs.Where(p => Runner != null && Runner.IsReady(p.slot)).ToList();

        var canHitNow = ready
            .Where(p => p.ability.Range >= distance)
            .OrderBy(p => p.ability.Range)
            .FirstOrDefault();

        if (canHitNow != null)
            return (canHitNow.ability, canHitNow.slot);

        var longest = pairs.OrderByDescending(p => p.ability.Range).First();
        return (longest.ability, longest.slot);
    }

    /// <summary>
    /// Decide movement/attack intent. Sets cmd.move here.
    /// Do NOT cast here; casting is performed by the processor using the command.
    /// </summary>
    protected abstract void UpdateBehavior(
        AbilityDefinition ability,
        float distance,
        Vector2 dir,
        ref ActorCommand cmd
    );

    /// <summary>
    /// Cast policy. Default: in range && runner ready && (optional LOS).
    /// </summary>
    protected virtual bool ShouldCast(AbilityDefinition ability, int slot, float distance)
    {
        if (ability == null || Runner == null)
            return false;
        if (requireLineOfSight && !HasLineOfSight(Target))
            return false;
        if (slot < 0)
            return false;
        return distance <= ability.Range && Runner.IsReady(slot);
    }

    /// <summary>
    /// Readiness policy. Default: global cooldown on Runner.
    /// Override if/when you add per-ability cooldowns.
    /// </summary>
    protected virtual bool IsAbilityReady(AbilityDefinition def)
    {
        if (Runner == null || def == null)
            return false;
        int slot = ResolveAbilitySlot(def);
        if (slot < 0)
            return false;

        return Runner.IsReady(slot);
    }

    private int ResolveAbilitySlot(AbilityDefinition ability)
    {
        var arr = Ctx?.Definition?.Abilities;
        if (arr == null || ability == null)
            return -1;
        for (int i = 0; i < arr.Length; i++)
            if (ReferenceEquals(arr[i], ability))
                return i;
        return -1;
    }

    // ---------- targeting ----------
    protected virtual Transform AcquireTarget()
    {
        float detect = Mathf.Max(0.1f, Ctx?.Definition?.DetectionRange ?? 8f);
        var hits = Physics2D.OverlapCircleAll(transform.position, detect, targetMask);

        float best = float.MaxValue;
        Transform bestT = null;

        foreach (var h in hits)
        {
            var tr = h.transform;
            if (!IsTargetValid(tr))
                continue;

            float d = Vector2.Distance(transform.position, tr.position);
            if (d < best)
            {
                if (!requireLineOfSight || HasLineOfSight(tr))
                {
                    best = d;
                    bestT = tr;
                }
            }
        }
        return bestT;
    }

    protected virtual bool IsTargetValid(Transform t)
    {
        if (t == null)
            return false;
        if (t.root == transform.root)
            return false; // skip self/children

        if (Team != null && t.TryGetComponent<TeamFeature>(out var otherTeam))
            if (Team.IsFriendly(otherTeam))
                return false;

        return true;
    }

    protected virtual bool HasLineOfSight(Transform t)
    {
        if (!requireLineOfSight)
            return true;
        Vector2 origin = transform.position;
        Vector2 dest = t.position;
        Vector2 dir = dest - origin;
        float dist = dir.magnitude;

        if (dist <= 0.001f)
            return true;

        var hit = Physics2D.Raycast(origin, dir.normalized, dist, losObstacles);
        return hit.collider == null;
    }

#if UNITY_EDITOR
    protected virtual void OnDrawGizmosSelected()
    {
        if (Ctx?.Definition != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, Ctx.Definition.DetectionRange);
        }

        if (Target != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, Target.position);
        }
    }
#endif
}
