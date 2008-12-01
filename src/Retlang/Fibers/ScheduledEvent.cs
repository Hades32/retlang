using System;

namespace Retlang.Fibers
{
    /// <summary>
    /// Event scheduled to be executed.
    /// </summary>
    public class ScheduledEvent
    {
        private readonly Action _action;
        private readonly long _firstIntervalInMs;
        private readonly long _regularIntervalInMs;
        private readonly bool _isRecurring;

        /// <summary>
        /// Schedule an event for a single execution
        /// </summary>
        /// <param name="runnable"></param>
        /// <param name="time"></param>
        public ScheduledEvent(Action runnable, long time)
            : this(runnable, time, -1)
        { }

        /// <summary>
        /// Schedule Recurring Event.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="firstIntervalInMs"></param>
        /// <param name="regularIntervalInMs"></param>
        public ScheduledEvent(Action action, long firstIntervalInMs, long regularIntervalInMs)
        {
            _action = action;
            _firstIntervalInMs = firstIntervalInMs;
            _regularIntervalInMs = regularIntervalInMs;
            _isRecurring = true;
        }

        /// <summary>
        /// The Action to be executed.
        /// </summary>
        public Action Action
        {
            get { return _action; }
        }

        /// <summary>
        /// The first time the event will be executed.
        /// </summary>
        public long FirstIntervalInMs
        {
            get { return _firstIntervalInMs; }
        }

        /// <summary>
        /// Regular interval in ms for action.
        /// </summary>
        public long RegularIntervalInMs
        {
            get { return _regularIntervalInMs; }
        }

        /// <summary>
        /// Determine if event will be fired on regular interval.
        /// </summary>
        public bool IsRecurring
        {
            get { return _isRecurring; }
        }

        /// <summary>
        /// Equality operation typically used for testing.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public override bool Equals(Object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            var scheduled = (ScheduledEvent)o;

            if (FirstIntervalInMs != scheduled.FirstIntervalInMs) return false;
            if (RegularIntervalInMs != scheduled.RegularIntervalInMs) return false;
            if (IsRecurring != scheduled.IsRecurring) return false;
            if (Action != null ? !Action.Equals(scheduled.Action) : scheduled.Action != null) return false;

            return true;
        }

        /// <summary>
        /// Hash
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            var result = (Action != null ? Action.GetHashCode() : 0);
            result = 31 * result + (int)(FirstIntervalInMs ^ (FirstIntervalInMs >> 32));
            result = 31 * result + (int)(RegularIntervalInMs ^ (RegularIntervalInMs >> 32));
            result = 31 * result + (IsRecurring ? 1 : 0);
            return result;
        }
    }
}
