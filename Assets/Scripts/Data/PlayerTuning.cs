using UnityEngine;

namespace LastDescent.ScriptableObjects
{
    [CreateAssetMenu(fileName = "PlayerTuning", menuName = "LastDescent/PlayerTuning")]
    public class PlayerTuning : ScriptableObject
    {
        [Header("Move")]
        public float moveSpeed = 6f;

        [Header("Aim/Rotation")]
        public float rotationSmoothTime = 0.06f;
    }
}
