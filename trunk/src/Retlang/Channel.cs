using System;
using System.Collections.Generic;
using Retlang;

namespace Retlang
{

    /// <summary>
    /// A channel provides a conduit for messages. It provides methods for publishing and subscribing to messages. 
    /// The class is thread safe.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IChannel<T>
    {
        ///<summary>
        /// Subscrive to messages on this channel. The provided action will be invoked via a command on the provided queue.
        ///</summary>
        ///<param name="queue">the target context to receive the message</param>
        ///<param name="receive"></param>
        ///<returns>Unsubscriber object</returns>
        IUnsubscriber Subscribe(ICommandQueue queue, Action<T> receive);

        /// <summary>
        /// Publish a message to all subscribers. Returns true if any subscribers are registered.
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        bool Publish(T msg);
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
                                                      Converter<T, K> keyResolver, Action<IDictionary<K, T>> receive, int intervalInMs);
    }


    ///<summary>
    /// Default Channel Implementation.
    ///</summary>
    ///<typeparam name="T"></typeparam>
    public class Channel<T>: IChannel<T>
    {
        private event Action<T> _subscribers;

        /// <summary>
        /// <see cref="IChannel{T}.Subscribe(ICommandQueue,Action{T})"/>
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="receive"></param>
        /// <returns></returns>
        public IUnsubscriber Subscribe(ICommandQueue queue, Action<T> receive)
        {
            ChannelSubscription<T> subscriber = new ChannelSubscription<T>(queue, receive);
            return SubscribeOnProducerThreads(subscriber.OnReceive);
        }

        internal void Unsubscribe(Action<T> toUnsubscribe)
        {
            _subscribers -= toUnsubscribe;
        }

        /// <summary>
        /// <see cref="IChannel{T}.Publish(T)"/>
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool Publish(T msg)
        {
            Action<T> evnt = _subscribers;
            if(evnt != null)
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
        /// <see cref="IChannel{T}.SubscribeToBatch(ICommandTimer,Action{IList{T}},int)"/>
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="receive"></param>
        /// <param name="intervalInMs"></param>
        /// <returns></returns>
        public IUnsubscriber SubscribeToBatch(ICommandTimer queue, Action<IList<T>> receive, int intervalInMs)
        {
            ChannelBatchSubscriber<T> batch = new ChannelBatchSubscriber<T>(queue, this, receive, intervalInMs);
            return SubscribeOnProducerThreads(batch.OnReceive);
        }

        /// <summary>
        /// <see cref="IChannel{T}.SubscribeToKeyedBatch{K}(ICommandTimer,Converter{T,K},Action{IDictionary{K,T}},int)"/>
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="queue"></param>
        /// <param name="keyResolver"></param>
        /// <param name="receive"></param>
        /// <param name="intervalInMs"></param>
        /// <returns></returns>
        public IUnsubscriber SubscribeToKeyedBatch<K>(ICommandTimer queue, 
            Converter<T, K> keyResolver, Action<IDictionary<K, T>> receive, int intervalInMs)
        {
            ChannelKeyedBatchSubscriber<K,T> batch = new ChannelKeyedBatchSubscriber<K,T>(keyResolver, receive, queue, intervalInMs);
            return SubscribeOnProducerThreads(batch.OnReceive);
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
    }

    internal class ChannelUnsubscriber<T>: IUnsubscriber
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