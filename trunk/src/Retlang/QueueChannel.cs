using System;
using System.Collections.Generic;
namespace Retlang
{
    internal class QueueConsumer<T>: IUnsubscriber
    {
        private bool _flushPending = false;
        private readonly ICommandQueue _target;
        private readonly Action<T> _callback;
        private readonly QueueChannel<T> _channel;
        public QueueConsumer(ICommandQueue target, Action<T> callback, QueueChannel<T> channel)
        {
            _target = target;
            _callback = callback;
            _channel = channel;
        }

        public void Signal()
        {
            lock (this)
            {
                if (_flushPending)
                {
                    return;
                }
                _target.Enqueue(ConsumeNext);
                _flushPending = true;
            }
        }

        private void ConsumeNext()
        {

            T msg;
            if (_channel.Pop(out msg))
            {
                _callback(msg);
            }
            lock (this)
            {
                if (_channel.Count == 0)
                {
                    _flushPending = false;
                }
                else
                {
                    _target.Enqueue(ConsumeNext);
                }
            }

        }

  
        public void Unsubscribe()
        {
            _channel.SignalEvent -= Signal;
        }


        internal void Subscribe()
        {
            _channel.SignalEvent += Signal;
        }
    }

    /// <summary>
    /// Creates a queue that will deliver a message to a single consumer. Load balancing can be achieved by creating 
    /// multiple subscribers to the queue.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IQueueChannel<T>
    {
        /// <summary>
        /// Subscribe to the queue.
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="onMessage"></param>
        /// <returns></returns>
        IUnsubscriber Subscribe(ICommandQueue queue, Action<T> onMessage);
        
        /// <summary>
        /// Pushes a message into the queue. Message will be processed by first available consumer.
        /// </summary>
        /// <param name="message"></param>
        void Publish(T message);
    }
    /// <summary>
    /// Default QueueChannel implementation. Once and only once delivery to first available consumer.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QueueChannel<T>: IQueueChannel<T>
    {
        private Queue<T> _queue = new Queue<T>();
        internal event Command SignalEvent;
        private Channel<T> _messageChannel = new Channel<T>();

        /// <summary>
        /// Subscribe to queue messages. 
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="onMessage"></param>
        /// <returns></returns>
        public IUnsubscriber Subscribe(ICommandQueue queue, Action<T> onMessage)
        {
            QueueConsumer<T> consumer = new QueueConsumer<T>(queue, onMessage, this);
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
                msg = default(T);
                return false;
            }
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
            Command onSignal = SignalEvent;
            if (onSignal != null)
            {
                onSignal();
            }
        }
    }
}

