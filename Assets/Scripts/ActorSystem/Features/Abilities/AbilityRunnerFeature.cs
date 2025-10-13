using UnityEngine;

[DisallowMultipleComponent]
public sealed class AbilityRunnerFeature : MonoBehaviour, IActorFeature
{
    public static float GlobalCooldown = 1f;
    private ActorContext _ctx;
    private TeamFeature _team;

    // Cooldown
    private float _gcdEndsAt;
    private float[] _slotEndsAt = System.Array.Empty<float>();

    // Preview fields
    private AbilityDefinition _currentAbility;
    private Vector2 _lastAimDir = Vector2.right;
#if UNITY_EDITOR
    private double _previewExpireAt;

    private static double Now() => UnityEditor.EditorApplication.timeSinceStartup;
#endif

    public void Initialize(ActorContext ctx)
    {
        _ctx = ctx;
        _team = GetComponent<TeamFeature>();
        int n = _ctx.Definition?.Abilities?.Length ?? 0;
        _slotEndsAt = n > 0 ? new float[n] : System.Array.Empty<float>();
        _gcdEndsAt = 0f;
    }

    public bool TryCast(AbilityDefinition def, int slot, Vector2 dir)
    {
        if (def == null)
            return false;
        if ((uint)slot >= (uint)_slotEndsAt.Length)
            return false;
        if (!IsReady(slot))
            return false;

        var aim = dir.sqrMagnitude > 1e-4f ? dir.normalized : (Vector2)transform.right;

        bool ok = def.TryCast(aim, _ctx, _team);
        if (!ok)
            return false;

        _gcdEndsAt = Time.time + GlobalCooldown;
        _slotEndsAt[slot] = Time.time + Mathf.Max(0f, def.Cooldown);
        _ctx.Events.Raise(new AbilityCastEvent { AbilityId = def.Id });

        // editor preview cache
        _currentAbility = def;
        _lastAimDir = dir;
#if UNITY_EDITOR
        _previewExpireAt = Now() + 2.0;
#endif

        return ok;
    }

    public void Tick(float dt) { }

    public void FixedTick(float fdt) { }

    public void Shutdown() { }

    public bool IsReady(int slot) =>
        Time.time >= _gcdEndsAt
        && (uint)slot < (uint)_slotEndsAt.Length
        && Time.time >= _slotEndsAt[slot];

#if UNITY_EDITOR
    public void SetPreview(AbilityDefinition def, Vector2 dir)
    {
        _currentAbility = def;
        _lastAimDir = dir.sqrMagnitude > 0.0001f ? dir.normalized : (Vector2)transform.right;
    }

    private void OnDrawGizmos()
    {
        DrawAbilityPreview();
    }

    private void OnDrawGizmosSelected()
    {
        DrawAbilityPreview();
    }

    private void DrawAbilityPreview()
    {
        if (_currentAbility == null)
            return;
        if (Now() > _previewExpireAt)
            return;

        var dir =
            _lastAimDir.sqrMagnitude > 0.0001f ? _lastAimDir.normalized : (Vector2)transform.right;
        _currentAbility.DrawEditorPreview(transform.position, dir, _ctx, transform);
    }
#endif
}
