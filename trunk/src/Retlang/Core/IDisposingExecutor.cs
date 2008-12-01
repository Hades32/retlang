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
        /// Append actions to end of queue.
        /// </summary>
        /// <param name="actions"></param>
        void EnqueueAll(params Action[] actions);

        /// <summary>
        /// Enqueue a single action.
        /// </summary>
        /// <param name="action"></param>
        void Enqueue(Action action);

        /// <summary>
        /// Register disposable.
        /// </summary>
        /// <param name="toAdd"></param>
        void Add(IDisposable toAdd);

        /// <summary>
        /// Remove Disposable.
        /// </summary>
        /// <param name="victim"></param>
        /// <returns></returns>
        bool Remove(IDisposable victim);
    
        /// <summary>
        /// Number of registered disposables.
        /// </summary>
        int DisposableCount { get; }
    }
}