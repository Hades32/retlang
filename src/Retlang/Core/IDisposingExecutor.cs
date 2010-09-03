using System;

namespace Retlang.Core
{
    /// <summary>
    /// Queues actions. Disposable objects can be added and removed. All dispose methods registered with the executor 
    /// will be invoked with the executor is disposed. This allows all subscriptions to be disposed when a fiber 
    /// is disposed.
    /// </summary>
    public interface IDisposingExecutor
    {
        /// <summary>
        /// Enqueue a single action.
        /// </summary>
        /// <param name="action"></param>
        void Enqueue(Action action);
    }
}