namespace Retlang
{
    public class LastSubscriber<T>
    {
        private readonly object _lock = new object();

        private readonly ICommandTimer _context;
        private readonly OnMessage<T> _target;
        private readonly int _flushIntervalInMs;

        private IMessageEnvelope<T> _pending = null;

        public LastSubscriber(OnMessage<T> target, ICommandTimer context, int flushIntervalInMs)
        {
            _context = context;
            _target = target;
            _flushIntervalInMs = flushIntervalInMs;
        }

        //received from message delivery thread
        public void ReceiveMessage(IMessageHeader header, T msg)
        {
            lock (_lock)
            {
                if (_pending == null)
                {
                    _context.Schedule(Flush, _flushIntervalInMs);
                }
                _pending = new MessageEnvelope<T>(header, msg);
            }
        }

        //flushed on process context thead
        public void Flush()
        {
            IMessageEnvelope<T> toReturn = ClearPending();
            if (toReturn != null)
            {
                _target(toReturn.Header, toReturn.Message);
            }
        }

        private IMessageEnvelope<T> ClearPending()
        {
            lock (_lock)
            {
                IMessageEnvelope<T> toReturn = _pending;
                _pending = null;
                return toReturn;
            }
        }

    }
}
