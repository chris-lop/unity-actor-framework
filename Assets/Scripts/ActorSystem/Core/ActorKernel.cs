using UnityEngine;

/// <summary>
/// Core component that manages an actor's lifecycle and coordinates its features.
///
/// Actor Setup Guide:
/// 1. Add ActorKernel to a GameObject
/// 2. Add feature components (Motor2DFeature, AbilityRunnerFeature, etc.)
/// 3. Add an input source (PlayerInputAdapter, AIInputSource, etc.)
/// 4. Add CommandProcessorFeature to connect input to features
/// 5. Assign an ActorDefinition ScriptableObject
///
/// Example Player Actor:
///   GameObject
///   ├─ ActorKernel (this)
///   ├─ Motor2DFeature
///   ├─ AbilityRunnerFeature
///   ├─ PlayerInputAdapter (IInputSource)
///   └─ CommandProcessorFeature
///
/// The kernel initializes all features on Awake and calls Tick/FixedTick/Shutdown on them.
/// Features can be added/removed at runtime and will be automatically managed.
/// </summary>
[DefaultExecutionOrder(-100)]
public sealed class ActorKernel : MonoBehaviour
{
    [SerializeField]
    private ActorDefinition definition;

    [SerializeField]
    private ActorEventBus eventBus = new();
    IActorFeature[] _features;
    public ActorContext Ctx { get; private set; }
    static int _idCounter;

    void Awake()
    {
        Ctx = new ActorContext(this, definition, eventBus, ++_idCounter);
        _features = GetComponents<IActorFeature>();
        foreach (var f in _features)
            f.Initialize(Ctx);
    }

    void Update()
    {
        float dt = Time.deltaTime;
        for (int i = 0; i < _features.Length; i++)
            _features[i].Tick(dt);
    }

    void FixedUpdate()
    {
        float fdt = Time.fixedDeltaTime;
        for (int i = 0; i < _features.Length; i++)
            _features[i].FixedTick(fdt);
    }

    void OnDestroy()
    {
        for (int i = 0; i < _features.Length; i++)
            _features[i].Shutdown();
    }

    public void SetDefinition(ActorDefinition def)
    { // runtime swap support (variants/upgrades)
        definition = def;
    }
}
