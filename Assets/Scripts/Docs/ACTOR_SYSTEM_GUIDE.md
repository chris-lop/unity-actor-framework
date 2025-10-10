# Actor System Integration Guide

## Overview

The actor system is a modular, feature-based architecture that allows you to create reusable actors (players, enemies, NPCs) that can be controlled by any input source (player input, AI, network, replay, etc.).

## Core Components

### 1. ActorKernel

The central manager that coordinates all features on an actor. It handles initialization, updates, and cleanup.

### 2. IActorFeature Interface

All actor features must implement this interface with four lifecycle methods:

- `Initialize(ActorContext)` - Called on Awake
- `Tick(float dt)` - Called every Update
- `FixedTick(float fdt)` - Called every FixedUpdate
- `Shutdown()` - Called on OnDestroy

### 3. ActorFeatureBase (Optional)

Abstract base class that provides default implementations of IActorFeature. Extend this to reduce boilerplate:

```csharp
public class MyFeature : ActorFeatureBase
{
    public override void Initialize(ActorContext ctx)
    {
        base.Initialize(ctx); // Sets Ctx property
        // Your initialization here
    }

    public override void Tick(float dt)
    {
        // Your per-frame logic here
    }
}
```

### 4. ActorContext

Read-only context passed to all features containing:

- `Kernel` - Reference to the ActorKernel
- `Definition` - The ActorDefinition ScriptableObject
- `Events` - The ActorEventBus for pub/sub
- `ActorId` - Unique ID for this actor instance

## Input System Architecture

### IInputSource<TCommand>

Generic interface for providing commands to actors. Any class implementing this can control an actor.

```csharp
public interface IInputSource<TCommand>
{
    TCommand ReadCommand();
}
```

### ActorCommand

The default command structure containing:

- `move` - Movement vector (-1 to 1)
- `aimWorld` - World position to aim at
- `attackPressed` - Attack button state (rising edge)

### ActorCommandProcessor

The bridge between input sources and actor features. It:

1. Reads ActorCommands from an IInputSource<ActorCommand> each frame
2. Dispatches commands to appropriate features (Motor2DFeature, AbilityRunnerFeature, etc.)
3. Can be extended or used as a template for custom command processors

Note: Unity doesn't support generic MonoBehaviours in the Inspector, so we use concrete classes instead. Each command type needs its own processor class (following the same pattern).

## Setting Up an Actor

### Player-Controlled Actor

1. Create a GameObject
2. Add required components:
   ```
   GameObject
   ├─ ActorKernel
   ├─ Rigidbody2D
   ├─ Motor2DFeature
   ├─ AbilityRunnerFeature
   ├─ TeamFeature
   ├─ PlayerInputAdapter
   └─ ActorCommandProcessor
   ```
3. Configure ActorKernel:
   - Assign an ActorDefinition ScriptableObject
4. Configure PlayerInputAdapter:
   - Assign InputActionReferences for move, aim, attack
   - Assign the world camera

### AI-Controlled Actor

Replace `PlayerInputAdapter` with your AI input source:

```
GameObject
├─ ActorKernel
├─ Rigidbody2D
├─ Motor2DFeature
├─ AbilityRunnerFeature
├─ TeamFeature
├─ AIInputSourceExample (or your custom AI)
└─ ActorCommandProcessor
```

### Network-Controlled Actor

Replace with your network input source:

```
GameObject
├─ ActorKernel
├─ Motor2DFeature
├─ AbilityRunnerFeature
├─ NetworkInputSource
└─ ActorCommandProcessor
```

## Creating Custom Input Sources

Implement `IInputSource<ActorCommand>`:

```csharp
public class MyInputSource : MonoBehaviour, IInputSource<ActorCommand>
{
    public ActorCommand ReadCommand()
    {
        var cmd = ActorCommand.Empty;

        // Generate your commands here
        cmd.move = CalculateMovement();
        cmd.aimWorld = CalculateAimTarget();
        cmd.attackPressed = ShouldAttack();

        return cmd;
    }
}
```

See `AIInputSourceExample.cs` for a complete reference implementation.

## Creating Custom Features

Extend `ActorFeatureBase`:

```csharp
public class HealthRegenFeature : ActorFeatureBase
{
    [SerializeField] private float regenRate = 1f;
    private AttributesFeature _attributes;

    public override void Initialize(ActorContext ctx)
    {
        base.Initialize(ctx);
        _attributes = GetComponent<AttributesFeature>();
    }

    public override void Tick(float dt)
    {
        if (_attributes != null)
        {
            _attributes.Heal(regenRate * dt);
        }
    }
}
```

## Custom Command Types

You can create specialized command types for different actor types. Unity doesn't support generic MonoBehaviours, so each command type needs its own concrete processor class.

**See `CustomCommandProcessorTemplate.cs` for a complete example.**

Quick example:

```csharp
// 1. Define your command struct
public struct VehicleCommand
{
    public float throttle;
    public float steering;
    public bool brake;
    public bool boost;
}

// 2. Implement an input source
public class VehicleInputAdapter : MonoBehaviour, IInputSource<VehicleCommand>
{
    public VehicleCommand ReadCommand()
    {
        var cmd = new VehicleCommand();
        cmd.throttle = Input.GetAxis("Vertical");
        cmd.steering = Input.GetAxis("Horizontal");
        cmd.brake = Input.GetKey(KeyCode.Space);
        cmd.boost = Input.GetKey(KeyCode.LeftShift);
        return cmd;
    }
}

// 3. Create a concrete processor (copy ActorCommandProcessor pattern)
[DisallowMultipleComponent]
public class VehicleCommandProcessor : ActorFeatureBase
{
    private IInputSource<VehicleCommand> _inputSource;
    private VehicleMotorFeature _motor;

    public override void Initialize(ActorContext ctx)
    {
        base.Initialize(ctx);
        _inputSource = GetComponent<IInputSource<VehicleCommand>>();
        _motor = GetComponent<VehicleMotorFeature>();

        if (_inputSource == null)
            Debug.LogWarning("No VehicleCommand input source found!");
    }

    public override void Tick(float dt)
    {
        if (_inputSource == null) return;

        var cmd = _inputSource.ReadCommand();
        ProcessCommand(cmd);
    }

    protected virtual void ProcessCommand(VehicleCommand cmd)
    {
        if (_motor != null)
        {
            _motor.ApplyThrottle(cmd.throttle);
            _motor.ApplySteering(cmd.steering);
            _motor.ApplyBrake(cmd.brake);
        }
    }
}
```

## Best Practices

1. **Keep Features Independent**: Features should not directly reference each other. Use ActorEventBus for communication when needed.

2. **Use Direct Calls for Commands**: ActorCommandProcessor uses direct method calls to features for performance. This is appropriate for per-frame command dispatching.

3. **Use Events for State Changes**: Use ActorEventBus for state changes like damage, death, ability casts, etc.

4. **Cache References**: Cache feature references in Initialize() to avoid GetComponent calls every frame.

5. **Handle Null Gracefully**: Features might be optional, so always check for null before using them.

6. **One Input Source Per Actor**: Each actor should have exactly one IInputSource component.

7. **One Command Processor Per Actor**: Each actor should have exactly one command processor (ActorCommandProcessor or custom).

8. **Concrete Classes for Unity**: Unity doesn't support generic MonoBehaviours, so create concrete processor classes for each command type.

## Extending the System

### Adding New Features

1. Create a new class extending `ActorFeatureBase`
2. Implement the lifecycle methods you need
3. Add it as a component to your actor GameObject

### Adding New Commands

1. Create a new command struct
2. Implement `IInputSource<YourCommand>`
3. Create a processor that handles your command type

### Switching Control at Runtime

```csharp
// Disable player input
playerInputAdapter.enabled = false;

// Enable AI input
aiInputSource.enabled = true;
```

The ActorCommandProcessor will automatically pick up the active input source.

## Troubleshooting

**Actor not responding to input:**

- Check that IInputSource component is attached to the same GameObject
- Check that ActorCommandProcessor is attached
- Check that Motor2DFeature or AbilityRunnerFeature are attached
- Look for warnings in the console from ActorCommandProcessor

**Input feels wrong:**

- Verify InputActionReferences are assigned in PlayerInputAdapter
- Check that input actions are enabled in your .inputactions file
- Verify camera reference in PlayerInputAdapter

**Abilities not working:**

- Check that ActorDefinition has abilities assigned
- Check that AbilityRunnerFeature is attached
- Check that hurtbox LayerMask is configured
- Look for debug rays in Scene view when attacking
