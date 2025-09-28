using UnityEngine;

namespace LastDescent.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class CharacterMotor2D : MonoBehaviour
    {
        private Rigidbody2D body;
        private Vector2 desiredVelocity;
        public float CurrentSpeed => body.linearVelocity.magnitude;

        private void Awake()
        {
            if (body == null) body = GetComponent<Rigidbody2D>();
        }

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
        public void SetDesiredVelocity(Vector2 v) => desiredVelocity = v;

        public void TeleportTo(Vector2 pos)
        {
            body.position = pos;
            body.linearVelocity = Vector2.zero;
        }

        private void FixedUpdate()
        {
            if (body == null) return;

            body.linearVelocity = desiredVelocity;
        }
    }
}
