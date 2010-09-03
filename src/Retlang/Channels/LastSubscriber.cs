using System;
using Retlang.Core;
using Retlang.Fibers;

namespace Retlang.Channels
{
    /// <summary>
    /// Subscribes to last event received on the channel. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LastSubscriber<T> : BaseSubscription<T>
    {
        private readonly object _lock = new object();

        private readonly IFiber _fiber;
        private readonly Action<T> _target;
        private readonly int _flushIntervalInMs;

        private bool _flushPending;
        private T _pending;

        /// <summary>
        /// New instance.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="fiber"></param>
        /// <param name="flushIntervalInMs"></param>
        public LastSubscriber(Action<T> target, IFiber fiber, int flushIntervalInMs)
        {
            _fiber = fiber;
            _target = target;
            _flushIntervalInMs = flushIntervalInMs;
        }

        ///<summary>
        /// Allows for the registration and deregistration of subscriptions
        ///</summary>
        public override ISubscriptions Subscriptions
        {
            get { return _fiber; }
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
                    _fiber.Schedule(Flush, _flushIntervalInMs);
                    _flushPending = true;
                }
                _pending = msg;
            }
        }

        /// <summary>
        /// Flushes on IFiber thread.
        /// </summary>
        private void Flush()
        {
            var toReturn = ClearPending();
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