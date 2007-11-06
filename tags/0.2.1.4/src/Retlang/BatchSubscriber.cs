using System.Collections.Generic;

namespace Retlang
{
    public class BatchSubscriber<T>
    {
        private readonly object _batchLock = new object();

        private readonly IProcessContext _context;
        private readonly On<IList<IMessageEnvelope<T>>> _target;
        private readonly int _flushIntervalInMs;

        private List<IMessageEnvelope<T>> _pending = null;

        public BatchSubscriber(On<IList<IMessageEnvelope<T>>> target, IProcessContext context, int flushIntervalInMs)
        {
            _context = context;
            _target = target;
            _flushIntervalInMs = flushIntervalInMs;
        }

        //received from message delivery thread
        public void ReceiveMessage(IMessageHeader header, T msg)
        {
            lock (_batchLock)
            {
                if (_pending == null)
                {
                    _pending = new List<IMessageEnvelope<T>>();
                    _context.Schedule(Flush, _flushIntervalInMs);
                }
                _pending.Add(new MessageEnvelope<T>(header, msg));
            }
        }

        //flushed on process context thead
        public void Flush()
        {
            IList < IMessageEnvelope < T> > toReturn = ClearPending();
            if (toReturn != null)
            {
                _target(toReturn);
            }
        }

        private IList<IMessageEnvelope<T>> ClearPending()
        {
            lock (_batchLock)
            {
                if (_pending == null || _pending.Count == 0)
                {
                    _pending = null;
                    return null;
                }
                IList<IMessageEnvelope<T>> toReturn = _pending;
                _pending = null;
                return toReturn;
            }
        }

    }
}