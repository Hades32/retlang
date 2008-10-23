using System;

namespace Retlang
{
    /// <summary>
    /// Subscribes to last event received on the channel. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ChannelLastSubscriber<T> : BaseSubscription<T>
    {
        private readonly object _lock = new object();

        private readonly ICommandTimer _context;
        private readonly Action<T> _target;
        private readonly int _flushIntervalInMs;

        private bool _flushPending;
        private T _pending;

        /// <summary>
        /// New instance.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="context"></param>
        /// <param name="flushIntervalInMs"></param>
        public ChannelLastSubscriber(Action<T> target, ICommandTimer context, int flushIntervalInMs)
        {
            _context = context;
            _target = target;
            _flushIntervalInMs = flushIntervalInMs;
        }

        /// <summary>
        /// Receives message from producer thread.
        /// </summary>
        /// <param name="msg"></param>
        protected override void OnMessageOnProducerThread(T msg)
        {
            lock (_lock)
            {
                if (!_flushPending)
                {
                    _context.Schedule(Flush, _flushIntervalInMs);
                    _flushPending = true;
                }
                _pending = msg;
            }
        }

        /// <summary>
        /// Flushes on IProcessTimer thread.
        /// </summary>
        private void Flush()
        {
            T toReturn = ClearPending();
            _target(toReturn);
        }

        private T ClearPending()
        {
            lock (_lock)
            {
                _flushPending = false;
                return _pending;
            }
        }
    }
}