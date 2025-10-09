using UnityEngine;

namespace LastDescent.Input
{
    /// <summary>
    /// One-frame actor intent from any input source (player device, AI, or network).
    /// This command structure is generic enough to work for players, enemies, and NPCs.
    /// </summary>
    public struct ActorCommand
    {
        /// <summary>Empty command with no input.</summary>
        public static readonly ActorCommand Empty = new();

        /// <summary>Movement input in world space (-1 to 1 on each axis).</summary>
        public Vector2 move;

        /// <summary>Target aim position in world coordinates.</summary>
        public Vector2 aimWorld;

        /// <summary>True if attack/ability button was pressed this frame (rising edge).</summary>
        public bool attackPressed;
    }
}