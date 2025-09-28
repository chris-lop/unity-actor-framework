namespace LastDescent.Input
{
    /// <summary>Use for non-local players or when input is disabled.</summary>
    public sealed class NullInputSource : IPlayerInputSource
    {
        public static readonly NullInputSource Instance = new NullInputSource();
        private NullInputSource() {}
        public PlayerCommand ReadCommand() => PlayerCommand.Empty;
    }
}