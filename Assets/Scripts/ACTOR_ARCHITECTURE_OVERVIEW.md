# Actor System Architecture - Quick Reference

## Core Pattern

```
┌─────────────────────────────────────────────────────┐
│                   ActorKernel                       │
│  (Manages lifecycle, initializes all features)      │
└─────────────────────────────────────────────────────┘
                         │
                         ├─ Provides ActorContext to all features
                         │
         ┌───────────────┼───────────────┬──────────────┐
         │               │               │              │
         ▼               ▼               ▼              ▼
  ┌──────────┐   ┌──────────┐   ┌──────────┐   ┌──────────┐
  │ Feature  │   │ Feature  │   │ Feature  │   │  Input   │
  │   #1     │   │   #2     │   │   #3     │   │ Processor│
  └──────────┘   └──────────┘   └──────────┘   └──────────┘
                                                      │
                                          Reads from  │
                                                      ▼
                                              ┌──────────────┐
                                              │ InputSource  │
                                              │ (IInputSource)│
                                              └──────────────┘
```

## Input Flow (Frame-by-Frame)

```
1. Input Device/AI/Network
           │
           ▼
2. IInputSource<ActorCommand>.ReadCommand()
           │
           ▼
3. ActorCommandProcessor.Tick()
           │
           ▼
4. ProcessCommand(cmd)
           │
           ├─► Motor2DFeature.Move(cmd.move)
           │
           └─► AbilityRunnerFeature.TryCast(ability, direction)
```

## Complete Actor Setup

### Player Actor

```
PlayerActor (GameObject)
├─ ActorKernel ...................... Manages all features
├─ Rigidbody2D ...................... Physics body
├─ Collider2D ....................... Physics collision
│
├─ Motor2DFeature ................... Movement capability
├─ AbilityRunnerFeature ............. Attack/ability capability
├─ TeamFeature ...................... Team identification
├─ AttributesFeature ................ Stats (optional)
├─ LifeFeature ...................... Health system (optional)
│
├─ PlayerInputAdapter ............... Reads keyboard/mouse/gamepad
└─ ActorCommandProcessor ............ Dispatches commands to features
```

### AI Actor

```
EnemyActor (GameObject)
├─ ActorKernel
├─ Rigidbody2D
├─ Collider2D
│
├─ Motor2DFeature
├─ AbilityRunnerFeature
├─ TeamFeature
│
├─ AIInputSourceExample ............. AI decision making
└─ ActorCommandProcessor ............ Dispatches commands to features
```

## Key Unity Limitation: No Generic MonoBehaviours

❌ **Won't Work:**

```csharp
// Generic MonoBehaviours can't be added in Unity Inspector
public class CommandProcessor<TCommand> : MonoBehaviour { }
```

✅ **Solution:**

```csharp
// Create concrete classes for each command type
public class ActorCommandProcessor : ActorFeatureBase
{
    private IInputSource<ActorCommand> _inputSource;
    // ...
}

public class VehicleCommandProcessor : ActorFeatureBase
{
    private IInputSource<VehicleCommand> _inputSource;
    // ...
}
```

## Creating Custom Command Types

### Pattern Template

```
1. Define Command Struct
   ↓
2. Create Input Source (implements IInputSource<YourCommand>)
   ↓
3. Create Command Processor (extends ActorFeatureBase)
   ↓
4. Create Custom Features (if needed)
   ↓
5. Attach all to GameObject
```

### Example: Vehicle System

```csharp
// 1. Command Struct
public struct VehicleCommand {
    public float throttle;
    public float steering;
    public bool brake;
}

// 2. Input Source
public class VehiclePlayerInput : MonoBehaviour, IInputSource<VehicleCommand> {
    public VehicleCommand ReadCommand() { /* ... */ }
}

// 3. Command Processor
public class VehicleCommandProcessor : ActorFeatureBase {
    private IInputSource<VehicleCommand> _inputSource;

    public override void Initialize(ActorContext ctx) {
        base.Initialize(ctx);
        _inputSource = GetComponent<IInputSource<VehicleCommand>>();
        // Cache feature references
    }

    public override void Tick(float dt) {
        if (_inputSource == null) return;
        var cmd = _inputSource.ReadCommand();
        ProcessCommand(cmd);
    }

    protected virtual void ProcessCommand(VehicleCommand cmd) {
        // Dispatch to features
    }
}
```

## Design Principles

✅ **DO:**

- Keep features independent
- Use direct method calls in command processors (performance)
- Use ActorEventBus for state changes (damage, death, etc.)
- Cache component references in Initialize()
- Make ProcessCommand() virtual for extensibility
- Handle missing components gracefully (null checks)

❌ **DON'T:**

- Make features depend on each other directly
- Use GetComponent every frame
- Create generic MonoBehaviour classes for Unity
- Forget to check for null references
- Put game logic in input sources (only read input)

## File Locations

**Core System:**

- `Core/ActorKernel.cs` - Main actor manager
- `Core/ActorContext.cs` - Shared context for features
- `Core/IActorFeature.cs` - Feature interface
- `Core/ActorFeatureBase.cs` - Feature base class (reduces boilerplate)
- `Core/ActorEventBus.cs` - Event system for actors

**Input System:**

- `Core/Input/IInputSource.cs` - Input source interface
- `Core/Input/ActorCommand.cs` - Standard command struct
- `Core/Input/AIInputSourceExample.cs` - Example AI input
- `Player/PlayerInputAdapter.cs` - Player input implementation

**Command Processing:**

- `Features/Commands/ActorCommandProcessor.cs` - Standard processor
- `Features/Commands/CustomCommandProcessorTemplate.cs` - Template for custom processors

**Other Features:**

- `Features/Movement/Motor2DFeature.cs` - 2D movement
- `Features/Abilities/AbilityRunnerFeature.cs` - Ability execution
- `Features/TeamFeature.cs` - Team identification
- `Features/Attributes/AttributesFeature.cs` - Stats system
- `Features/Attributes/LifeFeature.cs` - Health system

## Quick Troubleshooting

| Problem                          | Solution                                                                    |
| -------------------------------- | --------------------------------------------------------------------------- |
| Actor not moving                 | Check Motor2DFeature is attached                                            |
| Actor not attacking              | Check AbilityRunnerFeature + ActorDefinition has abilities                  |
| No input response                | Check IInputSource + ActorCommandProcessor are both attached                |
| "No IInputSource found" warning  | Attach input source to same GameObject as processor                         |
| Can't add processor in Inspector | Make sure you're using ActorCommandProcessor (concrete class) not a generic |

## Next Steps

1. Attach ActorKernel to your actor GameObject
2. Add required features (Motor2D, AbilityRunner, etc.)
3. Add an input source (PlayerInputAdapter or AIInputSource)
4. Add ActorCommandProcessor
5. Assign ActorDefinition ScriptableObject to ActorKernel
6. Configure input source (input actions, camera, etc.)
7. Press Play!

For detailed information, see `ACTOR_SYSTEM_GUIDE.md`
