using UnityEngine;

public abstract class AbilityDefinition : ScriptableObject
{
    public string Id = "Basic";
    public float Cooldown = 0.5f;
    public float Damage = 10f;
    public float Range = 1f;
    public LayerMask HurtboxMask;

    public abstract bool TryCast(Vector2 aimDir, ActorContext ctx, TeamFeature team);

    public abstract void DrawEditorPreview(
        Vector3 origin,
        Vector2 dir,
        ActorContext ctx,
        Transform caster
    );
}
