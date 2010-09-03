using System;
using Retlang.Core;

namespace Retlang.Fibers
{
    /// <summary>
    /// Queues pending events for the fiber.
    /// </summary>
    public interface IFiber : ISubscriptions, IDisposingExecutor, IScheduler, IDisposable
    {
        /// <summary>
        /// Start consuming events.
        /// </summary>
        void Start();
    }
}
