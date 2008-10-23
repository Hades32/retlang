using System;

namespace Retlang.Core
{
    public interface IDisposingExecutor
    {
        /// <summary>
        /// Append commands to end of queue.
        /// </summary>
        /// <param name="commands"></param>
        void EnqueueAll(params Command[] commands);

        /// <summary>
        /// Enqueue a single command.
        /// </summary>
        /// <param name="command"></param>
        void Enqueue(Command command);

        void Add(IDisposable toAdd);
        bool Remove(IDisposable victim);
    
        int DisposableCount { get; }
    }
}