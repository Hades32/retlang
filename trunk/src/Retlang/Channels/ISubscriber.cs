using System;
using System.Collections.Generic;
using Retlang.Core;

namespace Retlang.Channels
{
    /// <summary>
    /// Channel subscription methods.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISubscriber<T>
    {
        ///<summary>
        /// Subscribe to messages on this channel. The provided action will be invoked via a Action on the provided executor.
        ///</summary>
        ///<param name="executor">the target executor to receive the message</param>
        ///<param name="receive"></param>
        ///<returns>Unsubscriber object</returns>
        IUnsubscriber Subscribe(IDisposingExecutor executor, Action<T> receive);

        /// <summary>
        /// Removes all subscribers.
        /// </summary>
        void ClearSubscribers();

        /// <summary>
        /// Subscribes to events on the channel in batch form. The events will be batched if the consumer is unable to process the events 
        /// faster than the arrival rate.
        /// </summary>
        /// <param name="scheduler">The target context to execute the action</param>
        /// <param name="receive"></param>
        /// <param name="intervalInMs">Time in Ms to batch events. If 0 events will be delivered as fast as consumer can process</param>
        /// <returns></returns>
        IUnsubscriber SubscribeToBatch(IScheduler scheduler, Action<IList<T>> receive, int intervalInMs);

        ///<summary>
        /// Batches events based upon keyed values allowing for duplicates to be dropped. 
        ///</summary>
        ///<param name="scheduler"></param>
        ///<param name="keyResolver"></param>
        ///<param name="receive"></param>
        ///<param name="intervalInMs"></param>
        ///<typeparam name="K"></typeparam>
        ///<returns></returns>
        IUnsubscriber SubscribeToKeyedBatch<K>(IScheduler scheduler, Converter<T, K> keyResolver, Action<IDictionary<K, T>> receive, int intervalInMs);

        /// <summary>
        /// Subscription that delivers the latest message to the consuming thread.  If a newer message arrives before the consuming thread
        /// has a chance to process the message, the pending message is replaced by the newer message. The old message is discarded.
        /// </summary>
        /// <param name="scheduler"></param>
        /// <param name="receive"></param>
        /// <param name="intervalInMs"></param>
        /// <returns></returns>
        IUnsubscriber SubscribeToLast(IScheduler scheduler, Action<T> receive, int intervalInMs);

        /// <summary>
        /// Subscribes to messages on producer threads. Action will be invoked on producer thread. Action must
        /// be thread safe.
        /// </summary>
        /// <param name="subscriber"></param>
        /// <returns></returns>
        IUnsubscriber SubscribeOnProducerThreads(Action<T> subscriber);

        /// <summary>
        /// Subscribes to events on producer threads. Subscriber could be called from multiple threads.
        /// </summary>
        /// <param name="subscriber"></param>
        /// <returns></returns>
        IUnsubscriber SubscribeOnProducerThreads(IProducerThreadSubscriber<T> subscriber);
    }
}
