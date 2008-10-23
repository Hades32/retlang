using System;
using Retlang.Core;

namespace Retlang.Fibers
{
    /// <summary>
    /// Queues pending events for the process.
    /// </summary>
    public interface IFiber : IDisposingExecutor, IScheduler, IDisposable
    {
        /// <summary>
        /// Start consuming events.
        /// </summary>
        void Start();
    }
}
