using System;

namespace Retlang.Core
{
    /// <summary>
    /// Methods for scheduling actions that will be executed in the future.
    /// </summary>
    public interface IScheduler
    {
        /// <summary>
        /// Schedules an action to be executed once.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="firstInMs"></param>
        /// <returns>a handle to cancel the timer.</returns>
        IDisposable Schedule(Action action, int firstInMs);

        /// <summary>
        /// Schedule an action to be executed on a recurring interval.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="firstInMs"></param>
        /// <param name="regularInMs"></param>
        /// <returns>a handle to cancel the timer.</returns>
        IDisposable ScheduleOnInterval(Action action, int firstInMs, int regularInMs);
    }
}
