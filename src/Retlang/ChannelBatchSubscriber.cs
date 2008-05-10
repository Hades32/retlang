using System;
using System.Collections.Generic;

namespace Retlang
{
    internal class ChannelBatchSubscriber<T> : IUnsubscriber
    {
        private readonly object _lock = new object();
        private readonly ICommandTimer _queue;
        private readonly Channel<T> _channel;
        private readonly Action<IList<T>> _receive;
        private readonly int _interval;
        private List<T> _pending;

        public ChannelBatchSubscriber(ICommandTimer queue, Channel<T> channel, Action<IList<T>> receive, int interval)
        {
            _queue = queue;
            _channel = channel;
            _receive = receive;
            _interval = interval;
        }

        public void OnReceive(T msg)
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
            IList<T> toFlush= null;
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

        public void Unsubscribe()
        {
            _channel.Unsubscribe(OnReceive);
        }
    }
}