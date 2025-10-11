public sealed class ActorContext
{
    public readonly ActorKernel Kernel;
    public readonly ActorDefinition Definition;
    public readonly ActorEventBus Events;
    public readonly int ActorId;

    public ActorContext(ActorKernel k, ActorDefinition def, ActorEventBus bus, int id)
    {
        Kernel = k;
        Definition = def;
        Events = bus;
        ActorId = id;
    }
}
