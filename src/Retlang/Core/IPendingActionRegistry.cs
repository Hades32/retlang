using System;

namespace Retlang.Core
{
    /// <summary>
    /// Stores and removes pending actions.
    /// </summary>
    public interface IPendingActionRegistry
    {
        /// <summary>
        /// Remove timer
        /// </summary>
        /// <param name="timer"></param>
        void Remove(ITimerControl timer);

        /// <summary>
        /// Queue event to target queue.
        /// </summary>
        /// <param name="action"></param>
        void EnqueueTask(Action action);
    }
}
