
namespace LastDescent.Gameplay.Animation
{
    /// <summary>Anything that can play hurt/die (and optional revive) implements this.</summary>
    public interface IHurtDieAnimBridge
    {
        void PlayHurt();
        void PlayDie();
        // Optional, implement only if needed:
        void PlayRevive() { }
    }
}
