using System;
using System.Collections.Generic;

namespace Retlang
{
    /// <summary>
    /// Channel subscription methods.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IChannelSubscriber<T>
    {
        ///<summary>
        /// Subscribe to messages on this channel. The provided action will be invoked via a command on the provided queue.
        ///</summary>
        ///<param name="queue">the target context to receive the message</param>
        ///<param name="receive"></param>
        ///<returns>Unsubscriber object</returns>
        IUnsubscriber Subscribe(ICommandQueue queue, Action<T> receive);

        /// <summary>
        /// Removes all subscribers.
        /// </summary>
        void ClearSubscribers();

        /// <summary>
        /// Subscribes to events on the channel in batch form. The events will be batched if the consumer is unable to process the events 
        /// faster than the arrival rate.
        /// </summary>
        /// <param name="queue">The target context to execute the action</param>
        /// <param name="receive"></param>
        /// <param name="intervalInMs">Time in Ms to batch events. If 0 events will be delivered as fast as consumer can process</param>
        /// <returns></returns>
        IUnsubscriber SubscribeToBatch(ICommandTimer queue, Action<IList<T>> receive, int intervalInMs);


        ///<summary>
        /// Batches events based upon keyed values allowing for duplicates to be dropped. 
        ///</summary>
        ///<param name="queue"></param>
        ///<param name="keyResolver"></param>
        ///<param name="receive"></param>
        ///<param name="intervalInMs"></param>
        ///<typeparam name="K"></typeparam>
        ///<returns></returns>
        IUnsubscriber SubscribeToKeyedBatch<K>(ICommandTimer queue,
                                               Converter<T, K> keyResolver, Action<IDictionary<K, T>> receive,
                                               int intervalInMs);

        /// <summary>
        /// Subscription that delivers the latest message to the consuming thread.  If a newer message arrives before the consuming thread
        /// has a chance to process the message, the pending message is replaced by the newer message. The old message is discarded.
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="receive"></param>
        /// <param name="intervalInMs"></param>
        /// <returns></returns>
        IUnsubscriber SubscribeToLast(ICommandTimer queue, Action<T> receive, int intervalInMs);

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

    /// <summary>
    /// Channel publishing interface.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IChannelPublisher<T>
    {
        /// <summary>
        /// Publish a message to all subscribers. Returns true if any subscribers are registered.
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        bool Publish(T msg);
    }

    /// <summary>
    /// A channel provides a conduit for messages. It provides methods for publishing and subscribing to messages. 
    /// The class is thread safe.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IChannel<T> : IChannelSubscriber<T>, IChannelPublisher<T>
    {
    }

    ///<summary>
    /// Default Channel Implementation. Methods are thread safe.
    ///</summary>
    ///<typeparam name="T"></typeparam>
    public class Channel<T> : IChannel<T>
    {
        private event Action<T> _subscribers;

        /// <summary>
        /// <see cref="IChannelSubscriber{T}.Subscribe(ICommandQueue,Action{T})"/>
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="receive"></param>
        /// <returns></returns>
        public IUnsubscriber Subscribe(ICommandQueue queue, Action<T> receive)
        {
            ChannelSubscription<T> subscriber = new ChannelSubscription<T>(queue, receive);
            return SubscribeOnProducerThreads(subscriber);
        }

        internal void Unsubscribe(Action<T> toUnsubscribe)
        {
            _subscribers -= toUnsubscribe;
        }

        /// <summary>
        /// <see cref="IChannelPublisher{T}.Publish(T)"/>
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
        /// <see cref="IChannelSubscriber{T}.SubscribeToBatch(ICommandTimer,Action{IList{T}},int)"/>
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="receive"></param>
        /// <param name="intervalInMs"></param>
        /// <returns></returns>
        public IUnsubscriber SubscribeToBatch(ICommandTimer queue, Action<IList<T>> receive, int intervalInMs)
        {
            ChannelBatchSubscriber<T> batch = new ChannelBatchSubscriber<T>(queue, this, receive, intervalInMs);
            return SubscribeOnProducerThreads(batch);
        }

        /// <summary>
        /// <see cref="IChannelSubscriber{T}.SubscribeToKeyedBatch{K}(ICommandTimer,Converter{T,K},Action{IDictionary{K,T}},int)"/>
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="queue"></param>
        /// <param name="keyResolver"></param>
        /// <param name="receive"></param>
        /// <param name="intervalInMs"></param>
        /// <returns></returns>
        public IUnsubscriber SubscribeToKeyedBatch<K>(ICommandTimer queue,
                                                      Converter<T, K> keyResolver, Action<IDictionary<K, T>> receive,
                                                      int intervalInMs)
        {
            ChannelKeyedBatchSubscriber<K, T> batch =
                new ChannelKeyedBatchSubscriber<K, T>(keyResolver, receive, queue, intervalInMs);
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
            return new ChannelUnsubscriber<T>(subscriber, this);
        }

        /// <summary>
        /// Subscription that delivers the latest message to the consuming thread.  If a newer message arrives before the consuming thread
        /// has a chance to process the message, the pending message is replaced by the newer message. The old message is discarded.
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="receive"></param>
        /// <param name="intervalInMs"></param>
        /// <returns></returns>
        public IUnsubscriber SubscribeToLast(ICommandTimer queue, Action<T> receive, int intervalInMs)
        {
            ChannelLastSubscriber<T> sub = new ChannelLastSubscriber<T>(receive, queue, intervalInMs);
            return SubscribeOnProducerThreads(sub);
        }
    }

    internal class ChannelUnsubscriber<T> : IUnsubscriber
    {
        private readonly Action<T> _receiveMethod;
        private readonly Channel<T> _channel;

        public ChannelUnsubscriber(Action<T> receiveMethod, Channel<T> channel)
        {
            _receiveMethod = receiveMethod;
            _channel = channel;
        }

        public void Unsubscribe()
        {
            _channel.Unsubscribe(_receiveMethod);
        }
    }
}