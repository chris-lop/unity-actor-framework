using LastDescent.Input;
using UnityEngine;

public sealed class ChaseAndStrikeBrain : AIBrainFeature
{
    [SerializeField]
    private float minStopBuffer = 0.05f; // avoids jitter at range edge

    protected override void UpdateBehavior(
        AbilityDefinition ability,
        float distance,
        Vector2 dir,
        ref ActorCommand cmd
    )
    {
        float desired = ability ? Mathf.Max(0.1f, ability.Range) : 1.5f;

        // Move until within range
        if (distance > desired - minStopBuffer)
            cmd.move = dir;
        else
            cmd.move = Vector2.zero;
    }
}
