using System;
using System.Collections.Generic;

namespace Retlang
{
    public class ChannelKeyedBatchSubscriber<K, T>: BaseSubscription<T>, IChannelSubscription<T>
    {
        private readonly object _batchLock = new object();

        private readonly ICommandTimer _context;
        private readonly Action<IDictionary<K, T>> _target;
        private readonly int _flushIntervalInMs;
        private readonly Converter<T, K> _keyResolver;

        private Dictionary<K, T> _pending = null;

        public ChannelKeyedBatchSubscriber(Converter<T, K> keyResolver,
                                           Action<IDictionary<K, T>> target,
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
        /// <param name="msg"></param>
        protected override void OnMessageOnProducerThread(T msg)
        {
                lock (_batchLock)
                {
                    K key = _keyResolver(msg);
                    if (_pending == null)
                    {
                        _pending = new Dictionary<K, T>();
                        _context.Schedule(Flush, _flushIntervalInMs);
                    }
                    _pending[key] = msg;
                }
          }

        /// <summary>
        /// Flushed from process thread
        /// </summary>
        public void Flush()
        {
            IDictionary<K, T> toReturn = ClearPending();
            if (toReturn != null)
            {
                _target(toReturn);
            }
        }

        private IDictionary<K, T> ClearPending()
        {
            lock (_batchLock)
            {
                if (_pending == null || _pending.Count == 0)
                {
                    _pending = null;
                    return null;
                }
                IDictionary<K, T> toReturn = _pending;
                _pending = null;
                return toReturn;
            }
        }
    }
}