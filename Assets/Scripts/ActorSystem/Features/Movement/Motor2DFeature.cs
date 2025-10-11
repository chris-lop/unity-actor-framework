using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class Motor2DFeature : MonoBehaviour, IActorFeature
{
    private Rigidbody2D _rb;
    private ActorContext _ctx;
    private float _baseSpeed;

    public float MoveSpeed { get; private set; }
    public Vector2 MoveDirection { get; private set; }
    public Vector2 AimDirection { get; private set; }

    public void Initialize(ActorContext ctx)
    {
        _ctx = ctx;
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _baseSpeed = ctx.Definition.AttributeSet.BaseMoveSpeed;
        MoveDirection = Vector2.zero;
        AimDirection = Vector2.right;
        MoveSpeed = 0f;
    }

    public void Move(Vector2 input)
    {
        // --- Velocity & movement direction ---
        if (input.sqrMagnitude > 0.0001f)
        {
            MoveDirection = input.normalized;
            _rb.linearVelocity = MoveDirection * _baseSpeed;
        }
        else
        {
            MoveDirection = Vector2.zero;
            _rb.linearVelocity = Vector2.zero;
        }

        // store current actual speed from Rigidbody
        MoveSpeed = _rb.linearVelocity.magnitude;
    }

    public void SetAim(Vector2 aimWorld)
    {
        // --- Aim direction ---
        Vector2 facing = aimWorld - (Vector2)transform.position;
        if (facing.sqrMagnitude > 0.0001f)
            AimDirection = facing.normalized;
    }

    public void Tick(float dt) { }

    public void FixedTick(float fdt) { }

    public void Shutdown()
    {
        _rb.linearVelocity = Vector2.zero;
        MoveSpeed = 0f;
        MoveDirection = Vector2.zero;
    }
}
