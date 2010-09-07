using System;

namespace Retlang.Core
{
    internal class RecurringEvent : IPendingEvent
    {
        private readonly IExecutionContext _executionContext;
        private readonly Action _toExecute;
        private readonly long _regularInterval;

        private long _expiration;
        private bool _canceled;

        public RecurringEvent(IExecutionContext executionContext, Action toExecute, 
            long scheduledTimeInMs, long regularInterval, long currentTime)
        {
            _expiration = currentTime + scheduledTimeInMs;
            _executionContext = executionContext;
            _toExecute = toExecute;
            _regularInterval = regularInterval;
        }

        public long Expiration
        {
            get { return _expiration; }
        }

        public IPendingEvent Execute(long currentTime)
        {
            if (!_canceled)
            {
                _executionContext.Enqueue(_toExecute);
                _expiration = currentTime + _regularInterval;
                return this;
            }
            return null;
        }

        public void Dispose()
        {
            _canceled = true;
        }
    }
}
