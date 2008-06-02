using System;

namespace Retlang
{
    /// <summary>
    /// Subscription for events on a channel.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ChannelSubscription<T> : BaseSubscription<T>
    {
        private readonly Action<T> _receiveMethod;
        private readonly ICommandQueue _targetQueue;

        /// <summary>
        /// Construct the subscription
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="receiveMethod"></param>
        public ChannelSubscription(ICommandQueue queue, Action<T> receiveMethod)
        {
            _receiveMethod = receiveMethod;
            _targetQueue = queue;
        }

        /// <summary>
        /// Receives the event and queues the execution on the target queue.
        /// </summary>
        /// <param name="msg"></param>
        protected override void OnMessageOnProducerThread(T msg)
        {
            Command asyncExec = delegate { _receiveMethod(msg); };
            _targetQueue.Enqueue(asyncExec);
        }
    }
}