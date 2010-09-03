namespace Retlang.Core
{
    /// <summary>
    /// A runable queue implementation.
    /// </summary>
    public interface IQueue : IContext
    {
        /// <summary>
        /// Start consuming actions.
        /// </summary>
        void Run();

        /// <summary>
        /// Stop consuming actions.
        /// </summary>
        void Stop();
    }
}
