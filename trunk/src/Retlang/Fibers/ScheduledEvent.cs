using System;
using Retlang.Core;

namespace Retlang.Fibers
{
    public class ScheduledEvent
    {
        private readonly Command _command;
        private readonly long _firstIntervalInMs;
        private readonly long _regularIntervalInMs;
        private readonly bool _isRecurring;

        public ScheduledEvent(Command runnable, long time)
            : this(runnable, time, -1)
        { }

        public ScheduledEvent(Command command, long firstIntervalInMs, long regularIntervalInMs)
        {
            _command = command;
            _firstIntervalInMs = firstIntervalInMs;
            _regularIntervalInMs = regularIntervalInMs;
            _isRecurring = true;
        }

        public Command Command
        {
            get { return _command; }
        }

        public long FirstIntervalInMs
        {
            get { return _firstIntervalInMs; }
        }

        public long RegularIntervalInMs
        {
            get { return _regularIntervalInMs; }
        }

        public bool IsRecurring
        {
            get { return _isRecurring; }
        }

        public override bool Equals(Object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            ScheduledEvent scheduled = (ScheduledEvent)o;

            if (FirstIntervalInMs != scheduled.FirstIntervalInMs) return false;
            if (RegularIntervalInMs != scheduled.RegularIntervalInMs) return false;
            if (IsRecurring != scheduled.IsRecurring) return false;
            if (Command != null ? !Command.Equals(scheduled.Command) : scheduled.Command != null) return false;

            return true;
        }

        public override int GetHashCode()
        {
            int result = (Command != null ? Command.GetHashCode() : 0);
            result = 31 * result + (int)(FirstIntervalInMs ^ (FirstIntervalInMs >> 32));
            result = 31 * result + (int)(RegularIntervalInMs ^ (RegularIntervalInMs >> 32));
            result = 31 * result + (IsRecurring ? 1 : 0);
            return result;
        }
    }
}
