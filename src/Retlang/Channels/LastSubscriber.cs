using System;
using Retlang.Core;
using Retlang.Fibers;

namespace Retlang.Channels
{
    /// <summary>
    /// Subscribes to last action received on the channel. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LastSubscriber<T> : BaseSubscription<T>
    {
        private readonly object _batchLock = new object();

        private readonly Action<T> _target;
        private readonly IFiber _fiber;
        private readonly long _intervalInMs;

        private bool _flushPending;
        private T _pending;

        /// <summary>
        /// New instance.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="fiber"></param>
        /// <param name="intervalInMs"></param>
        public LastSubscriber(Action<T> target, IFiber fiber, long intervalInMs)
        {
            _fiber = fiber;
            _target = target;
            _intervalInMs = intervalInMs;
        }

        ///<summary>
        /// Allows for the registration and deregistration of subscriptions
        ///</summary>
        public override ISubscriptionRegistry Subscriptions
        {
            get { return _fiber; }
        }

        /// <summary>
        /// Receives message from producer thread.
        /// </summary>
        /// <param name="msg"></param>
        protected override void OnMessageOnProducerThread(T msg)
        {
            lock (_batchLock)
            {
                if (!_flushPending)
                {
                    _fiber.Schedule(Flush, _intervalInMs);
                    _flushPending = true;
                }
                _pending = msg;
            }
        }

        private void Flush()
        {
            _target(ClearPending());
        }

        private T ClearPending()
        {
            lock (_batchLock)
            {
                _flushPending = false;
                return _pending;
            }
        }
    }
}