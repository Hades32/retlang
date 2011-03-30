using System;
using System.Collections.Generic;
using Retlang.Fibers;

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
        ///<param name="fiber">the target executor to receive the message</param>
        ///<param name="receive"></param>
        ///<returns>Unsubscriber object</returns>
        IDisposable Subscribe(IFiber fiber, Action<T> receive);

        /// <summary>
        /// Subscribes to actions on the channel in batch form. The events will be batched if the consumer is unable to process the events 
        /// faster than the arrival rate.
        /// </summary>
        /// <param name="fiber">The target context to execute the action</param>
        /// <param name="receive"></param>
        /// <param name="intervalInMs">Time in Ms to batch actions. If 0 events will be delivered as fast as consumer can process</param>
        /// <returns></returns>
        IDisposable SubscribeToBatch(IFiber fiber, Action<IList<T>> receive, long intervalInMs);

        ///<summary>
        /// Batches actions based upon keyed values allowing for duplicates to be dropped. 
        ///</summary>
        ///<param name="fiber"></param>
        ///<param name="keyResolver"></param>
        ///<param name="receive"></param>
        ///<param name="intervalInMs"></param>
        ///<typeparam name="K"></typeparam>
        ///<returns></returns>
        IDisposable SubscribeToKeyedBatch<K>(IFiber fiber, Converter<T, K> keyResolver, Action<IDictionary<K, T>> receive, long intervalInMs);

        /// <summary>
        /// Subscription that delivers the latest message to the consuming thread.  If a newer message arrives before the consuming thread
        /// has a chance to process the message, the pending message is replaced by the newer message. The old message is discarded.
        /// </summary>
        /// <param name="fiber"></param>
        /// <param name="receive"></param>
        /// <param name="intervalInMs"></param>
        /// <returns></returns>
        IDisposable SubscribeToLast(IFiber fiber, Action<T> receive, long intervalInMs);

        /// <summary>
        /// Removes all subscribers.
        /// </summary>
        void ClearSubscribers();
    }
}
