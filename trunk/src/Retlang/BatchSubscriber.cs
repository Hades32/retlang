using System.Collections.Generic;

namespace Retlang
{
    public class BatchSubscriber<T>
    {
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

        public void ReceiveMessage(IMessageHeader header, T msg)
        {
            if (_pending == null)
            {
                _pending = new List<IMessageEnvelope<T>>();
                _context.Schedule(Flush, _flushIntervalInMs);
            }
            _pending.Add(new MessageEnvelope<T>(header, msg));
        }

        public void Flush()
        {
            if (_pending == null || _pending.Count == 0)
            {
                _pending = null;
                return;
            }
            IList<IMessageEnvelope<T>> toReturn = _pending;
            _pending = null;
            _target(toReturn);
        }
    }
}