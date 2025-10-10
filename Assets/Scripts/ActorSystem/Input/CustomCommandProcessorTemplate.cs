using UnityEngine;
using LastDescent.Input;

/// <summary>
/// TEMPLATE: Example showing how to create a custom command processor for custom command types.
/// 
/// To create your own command processor:
/// 1. Define your custom command struct (e.g., VehicleCommand, MechCommand, etc.)
/// 2. Implement IInputSource&lt;YourCommand&gt; for your input sources
/// 3. Copy this template and replace VehicleCommand with your command type
/// 4. Implement the ProcessCommand method to dispatch to your custom features
/// 
/// This template is commented out but demonstrates the pattern for extensibility.
/// </summary>

// Example custom command type
/*
public struct VehicleCommand
{
    public float throttle;      // -1 to 1
    public float steering;      // -1 to 1
    public bool brake;
    public bool boost;
}
*/

// Example custom command processor
/*
[DisallowMultipleComponent]
public class VehicleCommandProcessor : ActorFeatureBase
{
    private IInputSource<VehicleCommand> _inputSource;
    private VehicleMotorFeature _motor;
    private VehicleBoostFeature _boost;

    public override void Initialize(ActorContext ctx)
    {
        base.Initialize(ctx);

        // Find the input source on the same GameObject
        _inputSource = GetComponent<IInputSource<VehicleCommand>>();
        if (_inputSource == null)
        {
            Debug.LogWarning($"[VehicleCommandProcessor] No IInputSource<VehicleCommand> found on {gameObject.name}.", this);
        }

        // Cache references to features we'll be controlling
        _motor = GetComponent<VehicleMotorFeature>();
        _boost = GetComponent<VehicleBoostFeature>();
    }

    public override void Tick(float dt)
    {
        if (_inputSource == null) return;

        // Read command from input source
        VehicleCommand command = _inputSource.ReadCommand();

        // Dispatch command to features
        ProcessCommand(command);
    }

    protected virtual void ProcessCommand(VehicleCommand cmd)
    {
        // Dispatch to your custom features
        if (_motor != null)
        {
            _motor.ApplyThrottle(cmd.throttle);
            _motor.ApplySteering(cmd.steering);
            _motor.ApplyBrake(cmd.brake);
        }

        if (_boost != null && cmd.boost)
        {
            _boost.TryActivate();
        }
    }
}
*/

/// <summary>
/// Key Pattern Principles:
/// 
/// 1. ONE command processor per actor - it's the coordinator
/// 2. ONE input source per actor - only one thing controls at a time
/// 3. Command processors FIND their input source automatically via GetComponent
/// 4. Use direct method calls to features for performance
/// 5. Make ProcessCommand() virtual so it can be overridden for customization
/// 6. Cache feature references in Initialize() to avoid per-frame GetComponent calls
/// 7. Handle missing components gracefully with null checks
/// 
/// This pattern maintains:
/// - Flexibility: Any IInputSource can control the actor
/// - Performance: Direct method calls, cached references
/// - Unity Compatibility: Concrete classes work in the Inspector
/// - Extensibility: Easy to create new command types and processors
/// </summary>
public class CustomCommandProcessorTemplate : MonoBehaviour
{
    // This class exists only for documentation purposes
    // Delete or comment it out once you understand the pattern
}

