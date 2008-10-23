using System;
using System.Collections.Generic;
using Retlang.Core;

namespace Retlang.Channels
{
    /// <summary>
    /// Batches events for the consuming thread.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BatchSubscriber<T> : BaseSubscription<T>
    {
        private readonly object _lock = new object();
        private readonly IScheduler _queue;
        private readonly Action<IList<T>> _receive;
        private readonly int _interval;
        private List<T> _pending;

        /// <summary>
        /// Construct new instance.
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="receive"></param>
        /// <param name="interval"></param>
        public BatchSubscriber(IScheduler queue, Action<IList<T>> receive, int interval)
        {
            _queue = queue;
            _receive = receive;
            _interval = interval;
        }

        /// <summary>
        /// Receives message and batches as needed.
        /// </summary>
        /// <param name="msg"></param>
        protected override void OnMessageOnProducerThread(T msg)
        {
            lock (_lock)
            {
                if (_pending == null)
                {
                    _pending = new List<T>();
                    _queue.Schedule(Flush, _interval);
                }
                _pending.Add(msg);
            }
        }

        private void Flush()
        {
            IList<T> toFlush = null;
            lock (_lock)
            {
                if (_pending != null)
                {
                    toFlush = _pending;
                    _pending = null;
                }
            }
            if (toFlush != null)
            {
                _receive(toFlush);
            }
        }
    }
}