namespace Retlang.Core
{
    internal class RecurringEvent : IPendingEvent
    {
        private readonly IDisposingExecutor _executor;
        private readonly Command _toExecute;
        private readonly long _regularInterval;

        private long _expiration;
        private bool _canceled;

        public RecurringEvent(IDisposingExecutor executor, Command toExecute, 
            long scheduledTimeInMs, long regularInterval, long currentTime)
        {
            _expiration = currentTime + scheduledTimeInMs;
            _executor = executor;
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
                _executor.Enqueue(_toExecute);
                _expiration = currentTime + _regularInterval;
                return this;
            }
            return null;
        }

        public void Cancel()
        {
            _canceled = true;
        }
    }
}
