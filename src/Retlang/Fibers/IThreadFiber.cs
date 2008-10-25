using System.Threading;

namespace Retlang.Fibers
{
    /// <summary>
    /// Fiber implementation backed by a dedicated thread.
    /// </summary>
    public interface IThreadFiber : IFiber
    {
        /// <summary>
        /// Willl Wait for Thread to cease execution before continuing.
        /// </summary>
        void Join();

        /// <summary>
        /// The backing thread for the Fiber.
        /// </summary>
        Thread Thread { get; }
    }
}
