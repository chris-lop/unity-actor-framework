using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class AbilityRunnerFeature : MonoBehaviour, IActorFeature
{
    private ActorContext _ctx;
    private TeamFeature _team;
    private float _cd;

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
    }

    public bool TryCast(AbilityDefinition def, Vector2 dir)
    {
        if (def == null || _cd > 0f)
            return false;

        dir = dir.sqrMagnitude > 0.0001f ? dir.normalized : (Vector2)transform.right;

        // cache for editor preview
        _currentAbility = def;
        _lastAimDir = dir;
#if UNITY_EDITOR
        _previewExpireAt = Now() + 2.0;
#endif

        // For now, always raise cast event
        // TODO: If applying buff or healing, should be conditional
        _ctx.Events.Raise(new AbilityCastEvent { AbilityId = def.Id });
        _cd = def.Cooldown;

        return def.TryCast(dir, _ctx, _team);
    }

    public void Tick(float dt)
    {
        if (_cd > 0f)
            _cd -= dt;
    }

    public void FixedTick(float fdt) { }

    public void Shutdown() { }

#if UNITY_EDITOR
    public void SetPreview(AbilityDefinition def, Vector2 dir)
    {
        _currentAbility = def;
        _lastAimDir = (dir.sqrMagnitude > 0.0001f ? dir.normalized : (Vector2)transform.right);
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
