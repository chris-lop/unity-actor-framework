public interface IActorFeature {
    void Initialize(ActorContext ctx);
    void Tick(float dt);
    void FixedTick(float fdt);
    void Shutdown();
}