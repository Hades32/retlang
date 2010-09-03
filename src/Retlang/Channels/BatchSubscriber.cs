using System;
using System.Collections.Generic;
using Retlang.Core;
using Retlang.Fibers;

namespace Retlang.Channels
{
    /// <summary>
    /// Batches actions for the consuming thread.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BatchSubscriber<T> : BaseSubscription<T>
    {
        private readonly object _lock = new object();
        private readonly IFiber _fiber;
        private readonly Action<IList<T>> _receive;
        private readonly int _interval;
        private List<T> _pending;

        /// <summary>
        /// Construct new instance.
        /// </summary>
        /// <param name="fiber"></param>
        /// <param name="receive"></param>
        /// <param name="interval"></param>
        public BatchSubscriber(IFiber fiber, Action<IList<T>> receive, int interval)
        {
            _fiber = fiber;
            _receive = receive;
            _interval = interval;
        }

        ///<summary>
        /// Allows for the registration and deregistration of subscriptions
        ///</summary>
        public override ISubscriptionRegistry Subscriptions
        {
            get { return _fiber; }
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
                    _fiber.Schedule(Flush, _interval);
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