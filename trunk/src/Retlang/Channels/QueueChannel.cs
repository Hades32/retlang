using System;
using System.Collections.Generic;
using Retlang.Core;

namespace Retlang.Channels
{
    /// <summary>
    /// Default QueueChannel implementation. Once and only once delivery to first available consumer.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QueueChannel<T>: IQueueChannel<T>
    {
        private readonly Queue<T> _queue = new Queue<T>();
        internal event Action SignalEvent;
  
        /// <summary>
        /// Subscribe to executor messages. 
        /// </summary>
        /// <param name="executor"></param>
        /// <param name="onMessage"></param>
        /// <returns></returns>
        public IDisposable Subscribe(IContext executor, Action<T> onMessage)
        {
            var consumer = new QueueConsumer<T>(executor, onMessage, this);
            consumer.Subscribe();
            return consumer;
        }

        internal bool Pop(out T msg)
        {
            lock (_queue)
            {
                if (_queue.Count > 0)
                {
                    msg = _queue.Dequeue();
                    return true;
                }
            }
            msg = default(T);
            return false;
        }

        internal int Count
        {
            get
            {
                lock (_queue)
                {
                    return _queue.Count;
                }
            }
        }

        /// <summary>
        /// Publish message onto queue. Notify consumers of message.
        /// </summary>
        /// <param name="message"></param>
        public void Publish(T message)
        {
            lock (_queue)
            {
                _queue.Enqueue(message);
            }
            var onSignal = SignalEvent;
            if (onSignal != null)
            {
                onSignal();
            }
        }
    }
}