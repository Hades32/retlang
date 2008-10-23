using System;
using System.Collections.Generic;
using Retlang.Core;

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
        /// <see cref="ISubscriber{T}.Subscribe(IDisposingExecutor,Action{T})"/>
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="receive"></param>
        /// <returns></returns>
        public IUnsubscriber Subscribe(IDisposingExecutor queue, Action<T> receive)
        {
            ChannelSubscription<T> subscriber = new ChannelSubscription<T>(queue, receive);
            return SubscribeOnProducerThreads(subscriber);
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
            Action<T> evnt = _subscribers;
            if (evnt != null)
            {
                evnt(msg);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Remove all subscribers.
        /// </summary>
        public void ClearSubscribers()
        {
            _subscribers = null;
        }

        /// <summary>
        /// <see cref="ISubscriber{T}.SubscribeToBatch(IScheduler,Action{IList{T}},int)"/>
        /// </summary>
        /// <param name="scheduler"></param>
        /// <param name="receive"></param>
        /// <param name="intervalInMs"></param>
        /// <returns></returns>
        public IUnsubscriber SubscribeToBatch(IScheduler scheduler, Action<IList<T>> receive, int intervalInMs)
        {
            BatchSubscriber<T> batch = new BatchSubscriber<T>(scheduler, receive, intervalInMs);
            return SubscribeOnProducerThreads(batch);
        }

        /// <summary>
        /// <see cref="ISubscriber{T}.SubscribeToKeyedBatch{K}(IScheduler,Converter{T,K},Action{IDictionary{K,T}},int)"/>
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="scheduler"></param>
        /// <param name="keyResolver"></param>
        /// <param name="receive"></param>
        /// <param name="intervalInMs"></param>
        /// <returns></returns>
        public IUnsubscriber SubscribeToKeyedBatch<K>(IScheduler scheduler,
                                                      Converter<T, K> keyResolver, Action<IDictionary<K, T>> receive,
                                                      int intervalInMs)
        {
            KeyedBatchSubscriber<K, T> batch =
                new KeyedBatchSubscriber<K, T>(keyResolver, receive, scheduler, intervalInMs);
            return SubscribeOnProducerThreads(batch);
        }

        /// <summary>
        /// Subscribes to events on producer threads. Subscriber could be called from multiple threads.
        /// </summary>
        /// <param name="subscriber"></param>
        /// <returns></returns>
        public IUnsubscriber SubscribeOnProducerThreads(IProducerThreadSubscriber<T> subscriber)
        {
            return SubscribeOnProducerThreads(subscriber.ReceiveOnProducerThread);
        }

        /// <summary>
        /// Subscribes an action to be executed for every event posted to the channel. Action should be thread safe. 
        /// Action may be invoked on multiple threads.
        /// </summary>
        /// <param name="subscriber"></param>
        /// <returns></returns>
        public IUnsubscriber SubscribeOnProducerThreads(Action<T> subscriber)
        {
            _subscribers += subscriber;
            return new Unsubscriber<T>(subscriber, this);
        }

        /// <summary>
        /// Subscription that delivers the latest message to the consuming thread.  If a newer message arrives before the consuming thread
        /// has a chance to process the message, the pending message is replaced by the newer message. The old message is discarded.
        /// </summary>
        /// <param name="scheduler"></param>
        /// <param name="receive"></param>
        /// <param name="intervalInMs"></param>
        /// <returns></returns>
        public IUnsubscriber SubscribeToLast(IScheduler scheduler, Action<T> receive, int intervalInMs)
        {
            LastSubscriber<T> sub = new LastSubscriber<T>(receive, scheduler, intervalInMs);
            return SubscribeOnProducerThreads(sub);
        }
    }
}