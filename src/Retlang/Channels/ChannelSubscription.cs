using System;
using Retlang.Core;
using Retlang.Fibers;

namespace Retlang.Channels
{
    /// <summary>
    /// Subscription for events on a channel.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ChannelSubscription<T> : BaseSubscription<T>
    {
        private readonly Action<T> _receiver;
        private readonly IFiber _fiber;

        /// <summary>
        /// Construct the subscription
        /// </summary>
        /// <param name="fiber"></param>
        /// <param name="receiver"></param>
        public ChannelSubscription(IFiber fiber, Action<T> receiver)
        {
            _fiber = fiber;
            _receiver = receiver;
        }

        ///<summary>
        /// Allows for the registration and deregistration of subscriptions
        ///</summary>
        public override ISubscriptions Subscriptions
        {
            get { return _fiber; }
        }

        /// <summary>
        /// Receives the event and queues the execution on the target executor.
        /// </summary>
        /// <param name="msg"></param>
        protected override void OnMessageOnProducerThread(T msg)
        {
            _fiber.Enqueue(() => _receiver(msg));
        }
    }
}