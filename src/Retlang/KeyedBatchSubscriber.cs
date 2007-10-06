using System.Collections.Generic;

namespace Retlang
{
    public delegate K ResolveKey<K, V>(IMessageHeader header, V value);

    public class KeyedBatchSubscriber<K, V>
    {
        private readonly IProcessContext _context;
        private readonly On<IDictionary<K, IMessageEnvelope<V>>> _target;
        private readonly int _flushIntervalInMs;
        private readonly ResolveKey<K, V> _keyResolver;

        private Dictionary<K, IMessageEnvelope<V>> _pending = null;

        public KeyedBatchSubscriber(
            ResolveKey<K, V> keyResolver,
            On<IDictionary<K, IMessageEnvelope<V>>> target,
            IProcessContext context, int flushIntervalInMs)
        {
            _keyResolver = keyResolver;
            _context = context;
            _target = target;
            _flushIntervalInMs = flushIntervalInMs;
        }

        public void ReceiveMessage(IMessageHeader header, V msg)
        {
            if (_pending == null)
            {
                _pending = new Dictionary<K, IMessageEnvelope<V>>();
                _context.Schedule(Flush, _flushIntervalInMs);
            }
            K key = _keyResolver(header, msg);
            _pending[key] = new MessageEnvelope<V>(header, msg);
        }

        public void Flush()
        {
            if (_pending == null || _pending.Count == 0)
            {
                _pending = null;
                return;
            }
            IDictionary<K, IMessageEnvelope<V>> toReturn = _pending;
            _pending = null;
            _target(toReturn);
        }
    }
}