using System;

namespace Retlang.Core
{
    internal class SingleEvent : IPendingEvent
    {
        private readonly IContext _context;
        private readonly Action _toExecute;
        private readonly long _expiration;
        private bool _canceled;

        public SingleEvent(IContext context, Action toExecute, long scheduledTimeInMs, long now)
        {
            _expiration = now + scheduledTimeInMs;
            _context = context;
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
                _context.Enqueue(_toExecute);
            }
            return null;
        }

        public void Dispose()
        {
            _canceled = true;
        }
    }
}
