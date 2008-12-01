using System;

namespace Retlang.Core
{
    /// <summary>
    /// Stores and removes pending commands.
    /// </summary>
    public interface IPendingCommandRegistry
    {
        /// <summary>
        /// Remove timer
        /// </summary>
        /// <param name="timer"></param>
        void Remove(ITimerControl timer);

        /// <summary>
        /// Queue event to target queue.
        /// </summary>
        /// <param name="command"></param>
        void EnqueueTask(Action command);
    }
}
