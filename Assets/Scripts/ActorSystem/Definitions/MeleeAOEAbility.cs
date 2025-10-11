using UnityEngine;

[CreateAssetMenu(menuName = "LastDescent/Ability/Melee AOE")]
public class MeleeAOEAbility : AbilityDefinition
{
    public float Radius = 1.5f;
    public float ForwardOffset = 0.5f;

    public override bool TryCast(Vector2 aimDir, ActorContext ctx, TeamFeature team)
    {
        aimDir =
            aimDir.sqrMagnitude > 0.0001f ? aimDir.normalized : (Vector2)ctx.Kernel.transform.right;
        Vector2 origin = (Vector2)ctx.Kernel.transform.position + aimDir * ForwardOffset;

        var hits = Physics2D.OverlapCircleAll(origin, Radius, HurtboxMask);
        foreach (var col in hits)
        {
            var targetKernel = col.GetComponentInParent<ActorKernel>();
            if (targetKernel == null || targetKernel == ctx.Kernel)
                continue;

            var targetTeam = targetKernel.GetComponent<TeamFeature>();
            if (team != null && targetTeam != null && team.Team == targetTeam.Team)
                continue;

            targetKernel.Ctx.Events.Raise(
                new DamageEvent { SourceId = ctx.ActorId, Amount = Damage }
            );
        }
        return true;
    }

    public override void DrawEditorPreview(
        Vector3 origin,
        Vector2 dir,
        ActorContext ctx,
        Transform caster
    )
    {
#if UNITY_EDITOR
        dir = dir.sqrMagnitude > 0.0001f ? dir.normalized : (Vector2)caster.right;
        var center = origin + (Vector3)(dir * ForwardOffset);

        // --- preview colors ---
        var lineCol = new Color(0.2f, 1f, 0.8f, 0.9f);
        var fillCol = new Color(0.2f, 1f, 0.8f, 0.12f);

        // --- line from caster to center + arrow head ---
        using (new UnityEditor.Handles.DrawingScope(lineCol))
        {
            UnityEditor.Handles.DrawAAPolyLine(3f, origin, center);

            var tip = center;
            var back = -(Vector3)dir;
            var a = Quaternion.AngleAxis(+25f, Vector3.forward) * back * 0.35f;
            var b = Quaternion.AngleAxis(-25f, Vector3.forward) * back * 0.35f;
            UnityEditor.Handles.DrawAAPolyLine(2f, tip, tip + a);
            UnityEditor.Handles.DrawAAPolyLine(2f, tip, tip + b);

            // AOE wire
            UnityEditor.Handles.DrawWireDisc(center, Vector3.forward, Radius);
        }

        // --- faint fill for AOE ---
        using (new UnityEditor.Handles.DrawingScope(fillCol))
        {
            UnityEditor.Handles.DrawSolidDisc(center, Vector3.forward, Radius);
        }

        // --- show which colliders would be hit ---
        // note: mirror of TryCast logic: layer mask + team filter.
        if (ctx != null)
        {
            var team = caster != null ? caster.GetComponent<TeamFeature>() : null;

            var hits = Physics2D.OverlapCircleAll((Vector2)center, Radius, HurtboxMask);
            foreach (var col in hits)
            {
                var targetKernel = col.GetComponentInParent<ActorKernel>();
                if (
                    targetKernel == null
                    || (caster != null && targetKernel == caster.GetComponent<ActorKernel>())
                )
                    continue;

                var targetTeam = targetKernel.GetComponent<TeamFeature>();
                if (team != null && targetTeam != null && team.Team == targetTeam.Team)
                    continue;

                // mark hit candidates
                var hp = (Vector3)targetKernel.transform.position;
                using (new UnityEditor.Handles.DrawingScope(new Color(1f, 0.2f, 0.2f, 0.85f)))
                {
                    UnityEditor.Handles.DrawWireDisc(hp, Vector3.forward, 0.35f);
                }
            }
        }

        // --- label with info ---
        UnityEditor.Handles.Label(
            center + Vector3.up * (Radius + 0.2f),
            $"Melee AOE\nr={Radius:0.##}, offset={ForwardOffset:0.##}"
        );
#endif
    }
}
