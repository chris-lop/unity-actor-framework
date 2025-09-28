using UnityEngine;

namespace LastDescent.ScriptableObjects
{
    [CreateAssetMenu(fileName = "PlayerTuning", menuName = "LastDescent/PlayerTuning")]
    public class PlayerTuning : ScriptableObject
    {
        [Header("Move")]
        public float moveSpeed = 6f;
        public float acceleration = 50f;
        public float deceleration = 70f;

        [Header("Aim/Rotation")]
        public float rotationSmoothTime = 0.06f;
    }
}
