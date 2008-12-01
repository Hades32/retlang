namespace Retlang.Core
{
    /// <summary>
    /// A runable queue implementation.
    /// </summary>
    public interface IActionExecutor : IDisposingExecutor
    {
        /// <summary>
        /// Consume events.
        /// </summary>
        void Run();

        /// <summary>
        /// Stop consuming events.
        /// </summary>
        void Stop();
    }
}
