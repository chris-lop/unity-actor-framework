using UnityEngine;

[RequireComponent(typeof(Motor2DFeature))]
public sealed class AnimationPresenter : MonoBehaviour, IActorFeature
{
    static readonly int MoveSpeedHash = Animator.StringToHash("MoveSpeed");
    static readonly int AttackTriggerHash = Animator.StringToHash("AttackTrigger");
    static readonly int HurtTriggerHash = Animator.StringToHash("HurtTrigger");
    static readonly int DieTriggerHash = Animator.StringToHash("DieTrigger");

    [SerializeField]
    Animator animator;

    [SerializeField]
    SpriteRenderer sprite;

    private ActorContext _ctx;
    private Motor2DFeature _motor;

    public void Initialize(ActorContext ctx)
    {
        _ctx = ctx;
        if (!animator)
            animator = GetComponentInChildren<Animator>();

        _motor = GetComponent<Motor2DFeature>();

        _ctx.Events.OnAbilityCast += e => animator.SetTrigger(AttackTriggerHash);
        _ctx.Events.OnDeathStarted += OnDeathStarted;
        _ctx.Events.OnDamage += _ => animator.SetTrigger(HurtTriggerHash);
    }

    public void Tick(float dt)
    {
        if (animator == null || _motor == null)
            return;

        animator.SetFloat(MoveSpeedHash, _motor.MoveSpeed);

        if (sprite)
        {
            var dir = _motor.AimDirection;
            if (Mathf.Abs(dir.x) > 0.01f)
                sprite.flipX = dir.x < 0f;
        }
    }

    public void FixedTick(float fdt) { }

    public void Shutdown()
    {
        if (_ctx != null)
            _ctx.Events.OnDeathStarted -= OnDeathStarted;
    }

    private void OnDeathStarted()
    {
        animator.SetTrigger(DieTriggerHash);
    }
}
