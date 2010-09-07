using System;

namespace Retlang.Core
{
    internal class SingleEvent : IPendingEvent
    {
        private readonly IExecutionContext _executionContext;
        private readonly Action _toExecute;
        private readonly long _expiration;
        private bool _canceled;

        public SingleEvent(IExecutionContext executionContext, Action toExecute, long scheduledTimeInMs, long now)
        {
            _expiration = now + scheduledTimeInMs;
            _executionContext = executionContext;
            _toExecute = toExecute;
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
            }
            return null;
        }

        public void Dispose()
        {
            _canceled = true;
        }
    }
}
