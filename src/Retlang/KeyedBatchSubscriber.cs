using System.Collections.Generic;

namespace Retlang
{
    /// <summary>
    /// Resolves a message to a key.
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    /// <param name="header"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public delegate K ResolveKey<K, V>(IMessageHeader header, V value);

    /// <summary>
    /// Batches messages and drops duplicates based upon the resolved key.
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    internal class KeyedBatchSubscriber<K, V>
    {
        private readonly object _batchLock = new object();

        private readonly ICommandTimer _context;
        private readonly On<IDictionary<K, IMessageEnvelope<V>>> _target;
        private readonly int _flushIntervalInMs;
        private readonly ResolveKey<K, V> _keyResolver;

        private Dictionary<K, IMessageEnvelope<V>> _pending = null;

        /// <summary>
        /// Constructs new instance.
        /// </summary>
        /// <param name="keyResolver"></param>
        /// <param name="target"></param>
        /// <param name="context"></param>
        /// <param name="flushIntervalInMs"></param>
        public KeyedBatchSubscriber(
            ResolveKey<K, V> keyResolver,
            On<IDictionary<K, IMessageEnvelope<V>>> target,
            ICommandTimer context, int flushIntervalInMs)
        {
            _keyResolver = keyResolver;
            _context = context;
            _target = target;
            _flushIntervalInMs = flushIntervalInMs;
        }

        /// <summary>
        /// received on delivery thread
        /// </summary>
        /// <param name="header"></param>
        /// <param name="msg"></param>
        public void ReceiveMessage(IMessageHeader header, V msg)
        {
            lock (_batchLock)
            {
                K key = _keyResolver(header, msg);
                if (_pending == null)
                {
                    _pending = new Dictionary<K, IMessageEnvelope<V>>();
                    _context.Schedule(Flush, _flushIntervalInMs);
                }
                _pending[key] = new MessageEnvelope<V>(header, msg);
            }
        }

        /// <summary>
        /// Flushed from process thread
        /// </summary>
        public void Flush()
        {
            IDictionary<K, IMessageEnvelope<V>> toReturn = ClearPending();
            if (toReturn != null)
            {
                _target(toReturn);
            }
        }

        private IDictionary<K, IMessageEnvelope<V>> ClearPending()
        {
            lock (_batchLock)
            {
                if (_pending == null || _pending.Count == 0)
                {
                    _pending = null;
                    return null;
                }
                IDictionary<K, IMessageEnvelope<V>> toReturn = _pending;
                _pending = null;
                return toReturn;
            }
        }
    }
}