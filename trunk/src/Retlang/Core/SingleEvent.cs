namespace Retlang.Core
{
    internal class SingleEvent : IPendingEvent
    {
        private readonly IDisposingExecutor _queue;
        private readonly Command _toExecute;
        private readonly long _expiration;
        private bool _canceled;

        public SingleEvent(IDisposingExecutor queue, Command toExecute, long scheduledTimeInMs, long now)
        {
            _expiration = now + scheduledTimeInMs;
            _queue = queue;
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
                _queue.Enqueue(_toExecute);
            }
            return null;
        }

        public void Cancel()
        {
            _canceled = true;
        }
    }
}
