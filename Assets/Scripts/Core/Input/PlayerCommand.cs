using UnityEngine;

namespace LastDescent.Input
{
    /// <summary>One-frame player intent (device, AI, or network).</summary>
    public struct PlayerCommand
    {
        public Vector2 move;
        public Vector2 aimWorld;

        public static PlayerCommand Empty => new PlayerCommand
        {
            move = Vector2.zero,
            aimWorld = Vector2.zero,
        };
    }
}