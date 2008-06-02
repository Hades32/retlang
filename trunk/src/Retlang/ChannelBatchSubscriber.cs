using System;
using System.Collections.Generic;

namespace Retlang
{
    /// <summary>
    /// Batches events for the consuming thread.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ChannelBatchSubscriber<T> : BaseSubscription<T>
    {
        private readonly object _lock = new object();
        private readonly ICommandTimer _queue;
        private readonly Channel<T> _channel;
        private readonly Action<IList<T>> _receive;
        private readonly int _interval;
        private List<T> _pending;

        /// <summary>
        /// Construct new instance.
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="channel"></param>
        /// <param name="receive"></param>
        /// <param name="interval"></param>
        public ChannelBatchSubscriber(ICommandTimer queue, Channel<T> channel, Action<IList<T>> receive, int interval)
        {
            _queue = queue;
            _channel = channel;
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