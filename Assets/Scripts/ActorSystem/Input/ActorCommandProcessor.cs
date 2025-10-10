using UnityEngine;
using LastDescent.Input;

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
public class ActorCommandProcessor : ActorFeatureBase
{
    private IInputSource<ActorCommand> _inputSource;
    private Motor2DFeature _motor;
    private AbilityRunnerFeature _abilityRunner;
    private AbilityDefinition _defaultAbility;

    public override void Initialize(ActorContext ctx)
    {
        base.Initialize(ctx);

        // Find the input source on the same GameObject
        _inputSource = GetComponent<IInputSource<ActorCommand>>();
        if (_inputSource == null)
        {
            Debug.LogWarning($"[ActorCommandProcessor] No IInputSource<ActorCommand> found on {gameObject.name}. Actor will not respond to input.", this);
        }

        // Cache references to features we'll be controlling
        _motor = GetComponent<Motor2DFeature>();
        _abilityRunner = GetComponent<AbilityRunnerFeature>();

        // Get default ability from actor definition (use first ability if available)
        if (ctx.Definition.Abilities != null && ctx.Definition.Abilities.Length > 0)
        {
            _defaultAbility = ctx.Definition.Abilities[0];
        }
    }

    public override void Tick(float dt)
    {
        if (_inputSource == null) return;

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
        // 1) Movement
        if (_motor != null && cmd.move.sqrMagnitude > 0.001f)
        {
            _motor.Move(cmd.move);
        }
        else if (_motor != null)
        {
            _motor.Move(Vector2.zero); // Stop movement
        }

        // 2) Attack/Ability
        if (_abilityRunner != null && cmd.attackPressed && _defaultAbility != null)
        {
            // Calculate direction from actor position to aim target
            Vector2 actorPos = transform.position;
            Vector2 aimDirection = (cmd.aimWorld - actorPos).normalized;

            _abilityRunner.TryCast(_defaultAbility, aimDirection);
        }
    }
}


