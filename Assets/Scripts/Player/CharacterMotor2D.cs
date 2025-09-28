using UnityEngine;

namespace LastDescent.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class CharacterMotor2D : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D body;
        [SerializeField] private float acceleration = 50f;
        [SerializeField] private float deceleration = 70f;

        private Vector2 _desiredVelocity;
        public float CurrentSpeed => body != null ? body.linearVelocity.magnitude : 0f;

        private void Reset()
        {
            body = GetComponent<Rigidbody2D>();
            if (body != null)
            {
                body.gravityScale = 0f;
                body.freezeRotation = true;
                body.interpolation = RigidbodyInterpolation2D.Interpolate;
            }
        }

        private void Awake()
        {
            if (body == null) body = GetComponent<Rigidbody2D>();
        }

        public void SetDesiredVelocity(Vector2 v) => _desiredVelocity = v;

        public void TeleportTo(Vector2 pos)
        {
            body.position = pos;
            body.linearVelocity = Vector2.zero;
        }

        private void FixedUpdate()
        {
            if (body == null) return;

            Vector2 vel = body.linearVelocity;
            Vector2 target = _desiredVelocity;

            // Choose accel/decel per-axis
            Vector2 delta = target - vel;
            float ax = Mathf.Abs(target.x) > Mathf.Abs(vel.x) ? acceleration : deceleration;
            float ay = Mathf.Abs(target.y) > Mathf.Abs(vel.y) ? acceleration : deceleration;

            vel.x = Mathf.MoveTowards(vel.x, target.x, ax * Time.fixedDeltaTime);
            vel.y = Mathf.MoveTowards(vel.y, target.y, ay * Time.fixedDeltaTime);

            body.linearVelocity = vel;
        }
    }
}
