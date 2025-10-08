using UnityEngine;

namespace LastDescent.Enemies
{
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "LastDescent/Enemy Config", order = 0)]
    public class EnemyConfig : ScriptableObject
    {
        [Header("Identity")]
        public string enemyName = "Imp";

        [Header("Movement")]
        [Min(0f)] public float moveSpeed = 2.5f;
        [Tooltip("How close (world units) before switching from chase to attack.")]
        [Min(0f)] public float attackRange = 1.0f;
        [Tooltip("How far the enemy can sense/choose a target.")]
        [Min(0f)] public float aggroRange = 8f;

        [Header("Combat")]
        [Min(0f)] public float contactDamagePerHit = 5f;
        [Min(0f)] public float attackCooldown = 0.8f;

        [Header("Targeting")]
        [Tooltip("Layers containing valid targets with LifeState (e.g., Player, Portal).")]
        public LayerMask targetLayers;

        [Header("Death/Despawn")]
        [Tooltip("Seconds after death before destroying the GameObject.")]
        [Min(0f)] public float despawnDelay = 2f;
    }
}
