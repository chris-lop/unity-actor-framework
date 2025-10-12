using LastDescent.Input;
using UnityEngine;

/// <summary>
/// Reads ActorCommands from an IInputSource and dispatches them to actor features.
/// This is the standard command processor for most actors (players, AI, network).
///
/// Attach this component alongside an IInputSource&lt;ActorCommand&gt; to enable control:
/// - PlayerInputAdapter for player control
/// - AIInputSourceExample for AI control
/// - NetworkInputSource for network control (future)
///
/// The processor automatically finds the input source and dispatches commands to
/// Motor2DFeature for movement and AbilityRunnerFeature for abilities.
///
/// For custom command types, create a similar processor following this pattern.
/// </summary>
[DisallowMultipleComponent]
public class ActorCommandProcessorFeature : ActorFeatureBase
{
    private IInputSource<ActorCommand> _inputSource;
    private Motor2DFeature _motor;
    private AbilityRunnerFeature _abilityRunner;

    public override void Initialize(ActorContext ctx)
    {
        base.Initialize(ctx);

        // Find the input source on the same GameObject
        _inputSource = GetComponent<IInputSource<ActorCommand>>();
        if (_inputSource == null)
        {
            Debug.LogWarning(
                $"[ActorCommandProcessor] No IInputSource<ActorCommand> found on {gameObject.name}. Actor will not respond to input.",
                this
            );
        }

        _motor = GetComponent<Motor2DFeature>();
        _abilityRunner = GetComponent<AbilityRunnerFeature>();
    }

    public override void Tick(float dt)
    {
        if (_inputSource == null)
            return;

        // Read command from input source
        ActorCommand command = _inputSource.ReadCommand();

        // Dispatch command to features
        ProcessCommand(command);
    }

    /// <summary>
    /// Process the ActorCommand and dispatch to appropriate features.
    /// Override this to customize command handling behavior.
    /// </summary>
    protected virtual void ProcessCommand(ActorCommand cmd)
    {
        // 0) Set aim
        _motor.SetAim(cmd.aimWorld);

        // 1) Movement
        if (_motor != null && cmd.move.sqrMagnitude > 0.001f)
        {
            _motor.Move(cmd.move);
        }
        else if (_motor != null)
        {
            _motor.Move(Vector2.zero);
        }

        // 2) Attack/Ability
        if (_abilityRunner != null && cmd.attackPressed)
        {
            AbilityDefinition ability = ResolveBySlot(cmd.requestedAbilitySlot);

            if (ability != null)
            {
                Vector2 actorPos = transform.position;
                Vector2 aimDir =
                    (cmd.aimWorld - actorPos).sqrMagnitude > 0.0001f
                        ? (cmd.aimWorld - actorPos).normalized
                        : (Vector2)transform.right;

                _abilityRunner.TryCast(ability, cmd.requestedAbilitySlot, aimDir);
            }
        }
    }

    private AbilityDefinition ResolveBySlot(int slot)
    {
        var arr = Ctx.Definition.Abilities;
        if (arr == null || slot < 0 || slot >= arr.Length)
            return null;
        return arr[slot];
    }
}
