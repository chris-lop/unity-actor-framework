
namespace LastDescent.Input
{
    /// <summary>
    /// Generic interface for input sources that provide commands to actors.
    /// Implement this to create different control schemes for actors.
    /// 
    /// Examples:
    /// - PlayerInputAdapter: Reads from Unity Input System (keyboard/mouse/gamepad)
    /// - AIInputSource: Generates commands based on AI behavior logic
    /// - NetworkInputSource: Receives commands from network packets
    /// - ReplayInputSource: Plays back recorded commands
    /// </summary>
    /// <typeparam name="TCommand">The command type that this source produces</typeparam>
    public interface IInputSource<TCommand> {
        /// <summary>
        /// Called each frame by CommandProcessorFeature to read the current command.
        /// Should return a fresh command representing the current input state.
        /// </summary>
        TCommand ReadCommand();
    }
}