using UnityEngine;

namespace LastDescent.Input
{
    /// <summary>One-frame player intent (device, AI, or network).</summary>
    public struct PlayerCommand
    {
        public static readonly PlayerCommand Empty = new();
        public Vector2 move;
        public Vector2 aimWorld;
        public bool attackPressed;
    }
}