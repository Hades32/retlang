using System;
using System.Collections.Generic;
using Retlang.Core;

namespace Retlang.Channels
{
    /// <summary>
    /// Channel subscription that drops duplicates based upon a key.
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="T"></typeparam>
    public class KeyedBatchSubscriber<K, T> : BaseSubscription<T>
    {
        private readonly object _batchLock = new object();

        private readonly IScheduler _context;
        private readonly Action<IDictionary<K, T>> _target;
        private readonly int _flushIntervalInMs;
        private readonly Converter<T, K> _keyResolver;

        private Dictionary<K, T> _pending;

        /// <summary>
        /// Construct new instance.
        /// </summary>
        /// <param name="keyResolver"></param>
        /// <param name="target"></param>
        /// <param name="context"></param>
        /// <param name="flushIntervalInMs"></param>
        public KeyedBatchSubscriber(Converter<T, K> keyResolver,
                                    Action<IDictionary<K, T>> target,
                                    IScheduler context, int flushIntervalInMs)
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
                var key = _keyResolver(msg);
                if (_pending == null)
                {
                    _pending = new Dictionary<K, T>();
                    _context.Schedule(Flush, _flushIntervalInMs);
                }
                _pending[key] = msg;
            }
        }

        /// <summary>
        /// Flushed from fiber
        /// </summary>
        public void Flush()
        {
            var toReturn = ClearPending();
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