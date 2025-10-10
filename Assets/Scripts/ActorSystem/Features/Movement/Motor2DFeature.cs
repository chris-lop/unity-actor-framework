using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class Motor2DFeature : MonoBehaviour, IActorFeature
{
    Rigidbody2D _rb;
    ActorContext _ctx;
    float _speed;

    public void Initialize(ActorContext ctx)
    {
        _ctx = ctx;
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _speed = ctx.Definition.AttributeSet.BaseMoveSpeed;
    }
    public void Move(Vector2 input)
    {
        _rb.linearVelocity = input * _speed;
    }
    public void Tick(float dt) { }
    public void FixedTick(float fdt) { }
    public void Shutdown() { _rb.linearVelocity = Vector2.zero; }
}
