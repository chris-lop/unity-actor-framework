using UnityEngine;

public sealed class AnimationEventHandler : MonoBehaviour
{
    private ActorContext _ctx;

    private void Awake()
    {
        var kernel = GetComponentInParent<ActorKernel>();
        _ctx = kernel.Ctx;
    }

    public void OnDeathAnimationFinished()
    {
        _ctx?.Events?.RaiseDeathFinished();
    }
}
