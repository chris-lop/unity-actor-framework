/// <summary>
/// Base class for actor features that provides default empty implementations.
/// Inherit from this to reduce boilerplate when only some lifecycle methods are needed.
/// </summary>
public abstract class ActorFeatureBase : UnityEngine.MonoBehaviour, IActorFeature
{
    protected ActorContext Ctx { get; private set; }

    public virtual void Initialize(ActorContext ctx)
    {
        Ctx = ctx;
    }

    public virtual void Tick(float dt) { }

    public virtual void FixedTick(float fdt) { }

    public virtual void Shutdown() { }
}


