using System;
using System.Collections.Generic;
using Retlang.Core;
using Retlang.Fibers;

namespace Retlang.Channels
{
    ///<summary>
    /// Default Channel Implementation. Methods are thread safe.
    ///</summary>
    ///<typeparam name="T"></typeparam>
    public class Channel<T> : IChannel<T>
    {
        private event Action<T> _subscribers;

        /// <summary>
        /// <see cref="ISubscriber{T}.Subscribe(IFiber,Action{T})"/>
        /// </summary>
        /// <param name="fiber"></param>
        /// <param name="receive"></param>
        /// <returns></returns>
        public IDisposable Subscribe(IFiber fiber, Action<T> receive)
        {
            return SubscribeOnProducerThreads(new ChannelSubscription<T>(fiber, receive));
        }

        /// <summary>
        /// <see cref="ISubscriber{T}.SubscribeToBatch(IFiber,Action{IList{T}},int)"/>
        /// </summary>
        /// <param name="fiber"></param>
        /// <param name="receive"></param>
        /// <param name="intervalInMs"></param>
        /// <returns></returns>
        public IDisposable SubscribeToBatch(IFiber fiber, Action<IList<T>> receive, int intervalInMs)
        {
            return SubscribeOnProducerThreads(new BatchSubscriber<T>(fiber, receive, intervalInMs));
        }

        /// <summary>
        /// <see cref="ISubscriber{T}.SubscribeToKeyedBatch{K}(IFiber,Converter{T,K},Action{IDictionary{K,T}},int)"/>
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="fiber"></param>
        /// <param name="keyResolver"></param>
        /// <param name="receive"></param>
        /// <param name="intervalInMs"></param>
        /// <returns></returns>
        public IDisposable SubscribeToKeyedBatch<K>(IFiber fiber, Converter<T, K> keyResolver, Action<IDictionary<K, T>> receive, int intervalInMs)
        {
            return SubscribeOnProducerThreads(new KeyedBatchSubscriber<K, T>(keyResolver, receive, fiber, intervalInMs));
        }

        /// <summary>
        /// Subscription that delivers the latest message to the consuming thread.  If a newer message arrives before the consuming thread
        /// has a chance to process the message, the pending message is replaced by the newer message. The old message is discarded.
        /// </summary>
        /// <param name="fiber"></param>
        /// <param name="receive"></param>
        /// <param name="intervalInMs"></param>
        /// <returns></returns>
        public IDisposable SubscribeToLast(IFiber fiber, Action<T> receive, int intervalInMs)
        {
            return SubscribeOnProducerThreads(new LastSubscriber<T>(receive, fiber, intervalInMs));
        }

        /// <summary>
        /// Subscribes to actions on producer threads. Subscriber could be called from multiple threads.
        /// </summary>
        /// <param name="subscriber"></param>
        /// <returns></returns>
        public IDisposable SubscribeOnProducerThreads(IProducerThreadSubscriber<T> subscriber)
        {
            return SubscribeOnProducerThreads(subscriber.ReceiveOnProducerThread, subscriber.Subscriptions);
        }

        /// <summary>
        /// Subscribes an action to be executed for every action posted to the channel. Action should be thread safe. 
        /// Action may be invoked on multiple threads.
        /// </summary>
        /// <param name="subscriber"></param>
        /// <param name="subscriptions"></param>
        /// <returns></returns>
        private IDisposable SubscribeOnProducerThreads(Action<T> subscriber, ISubscriptionRegistry subscriptions)
        {
            _subscribers += subscriber;

            var unsubscriber = new Unsubscriber<T>(subscriber, this, subscriptions);
            subscriptions.RegisterSubscription(unsubscriber);

            return unsubscriber;
        }

        internal void Unsubscribe(Action<T> toUnsubscribe)
        {
            _subscribers -= toUnsubscribe;
        }

        /// <summary>
        /// <see cref="IPublisher{T}.Publish(T)"/>
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool Publish(T msg)
        {
            var evnt = _subscribers;
            if (evnt != null)
            {
                evnt(msg);
                return true;
            }
            return false;
        }

        ///<summary>
        /// Number of subscribers
        ///</summary>
        public int NumSubscribers
        {
            get { return _subscribers == null ? 0 : _subscribers.GetInvocationList().Length; }
        }

        /// <summary>
        /// Remove all subscribers.
        /// </summary>
        public void ClearSubscribers()
        {
            _subscribers = null;
        }
    }
}