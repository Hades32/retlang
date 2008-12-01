using System;

namespace Retlang.Core
{
    /// <summary>
    /// Methods for schedule events that will be executed in the future.
    /// </summary>
    public interface IScheduler
    {
        /// <summary>
        /// Schedules an event to be executes once.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="timeTilEnqueueInMs"></param>
        /// <returns>a controller to cancel the event.</returns>
        ITimerControl Schedule(Action action, long timeTilEnqueueInMs);

        /// <summary>
        /// Schedule an event on a recurring interval.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="firstInMs"></param>
        /// <param name="regularInMs"></param>
        /// <returns>controller to cancel timer.</returns>
        ITimerControl ScheduleOnInterval(Action action, long firstInMs, long regularInMs);
    }
}
