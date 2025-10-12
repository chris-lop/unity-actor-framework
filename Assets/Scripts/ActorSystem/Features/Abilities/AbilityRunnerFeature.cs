using UnityEngine;

[DisallowMultipleComponent]
public sealed class AbilityRunnerFeature : MonoBehaviour, IActorFeature
{
    private ActorContext _ctx;
    private TeamFeature _team;
    private float[] _cdBySlot;

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
        _cdBySlot = n > 0 ? new float[n] : System.Array.Empty<float>();
    }

    public bool TryCast(AbilityDefinition def, int slot, Vector2 dir)
    {
        if (def == null)
            return false;
        if (!IsReady(slot))
            return false;

        dir = dir.sqrMagnitude > 0.0001f ? dir.normalized : (Vector2)transform.right;

        bool ok = def.TryCast(
            dir.sqrMagnitude > 0.0001f ? dir.normalized : (Vector2)transform.right,
            _ctx,
            GetComponent<TeamFeature>()
        );

        if (ok)
        {
            _cdBySlot[slot] = Mathf.Max(0f, def.Cooldown);
            _ctx.Events.Raise(new AbilityCastEvent { AbilityId = def.Id });

            // cache for editor preview
            _currentAbility = def;
            _lastAimDir = dir;
#if UNITY_EDITOR
            _previewExpireAt = Now() + 2.0;
#endif
        }
        return ok;
    }

    public void Tick(float dt)
    {
        for (int i = 0; i < _cdBySlot.Length; i++)
            if (_cdBySlot[i] > 0f)
                _cdBySlot[i] -= dt;
    }

    public void FixedTick(float fdt) { }

    public void Shutdown() { }

    public bool IsReady(int slot)
    {
        return slot >= 0 && slot < _cdBySlot.Length && _cdBySlot[slot] <= 0f;
    }

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
