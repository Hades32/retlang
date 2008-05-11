namespace Retlang
{
    /// <summary>
    /// Subscribes to last event from queue. Old events are discarded.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LastSubscriber<T>
    {
        private readonly object _lock = new object();

        private readonly ICommandTimer _context;
        private readonly OnMessage<T> _target;
        private readonly int _flushIntervalInMs;

        private IMessageEnvelope<T> _pending = null;

        /// <summary>
        /// New instance.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="context"></param>
        /// <param name="flushIntervalInMs"></param>
        public LastSubscriber(OnMessage<T> target, ICommandTimer context, int flushIntervalInMs)
        {
            _context = context;
            _target = target;
            _flushIntervalInMs = flushIntervalInMs;
        }

        /// <summary>
        /// Receives message from delivery thread.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="msg"></param>
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

        /// <summary>
        /// Flushes on IProcessBus thread.
        /// </summary>
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
