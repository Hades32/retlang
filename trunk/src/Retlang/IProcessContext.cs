using System.Collections.Generic;

namespace Retlang
{

    /// <summary>
    /// Provides methods for publishing and subscribing to events. Events will be delivered sequentially. 
    /// </summary>
    public interface IProcessBus : IObjectPublisher, ICommandQueue, ICommandTimer
    {
        /// <summary>
        /// Callback from any and all publishing threads. Not Thread Safe.
        /// Will only happen if the max size of the queue and the max wait times are set.
        /// </summary>
        event OnQueueFull QueueFullEvent;

        /// <summary>
        /// Publish the message.
        /// </summary>
        /// <param name="toPublish"></param>
        void Publish(ITransferEnvelope toPublish);

        /// <summary>
        /// Posts a message to this context only. The message is not broadcast.
        /// Returns true if a subscriber is found.
        /// </summary>
        bool Post(object topic, object msg, object replyToTopic);

        /// <summary>
        /// A batch subscription that drops duplicates based upon the ResolveKey delegate provided.
        /// </summary>
        IUnsubscriber SubscribeToKeyedBatch<K, V>(ITopicMatcher topic, ResolveKey<K, V> keyResolver,
                                                  On<IDictionary<K, IMessageEnvelope<V>>> target,
                                                  int minBatchIntervalInMs);

        /// <summary>
        /// A batch subscription that delivers a list of events to the subscriber.
        /// </summary>
        IUnsubscriber SubscribeToBatch<T>(ITopicMatcher topic, On<IList<IMessageEnvelope<T>>> msg,
                                          int minBatchIntervalInMs);

        /// <summary>
        /// Batch subscription that only delivers the last event to the target delegate.
        /// </summary>
        IUnsubscriber SubscribeToLast<T>(ITopicMatcher topic, OnMessage<T> msg, int minBatchIntervalInMs);

        /// <summary>
        /// Subscribe for events on based upon the matcher and generic type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="topic"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        IUnsubscriber Subscribe<T>(ITopicMatcher topic, OnMessage<T> msg);

        /// <summary>
        /// Send a request using the provided wrapped message
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="env"></param>
        /// <returns></returns>
        IRequestReply<T> SendRequest<T>(ITransferEnvelope env);

        /// <summary>
        /// Send request message.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="topic"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        IRequestReply<T> SendRequest<T>(object topic, object msg);
        
        /// <summary>
        /// Send async request. The timeout command will be invoked if a reply is not returned within the timeout period.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="topic"></param>
        /// <param name="msg"></param>
        /// <param name="onReply"></param>
        /// <param name="onTimeout"></param>
        /// <param name="requestTimeout"></param>
        void SendAsyncRequest<T>(object topic, object msg, OnMessage<T> onReply, Command onTimeout, long requestTimeout);

        /// <summary>
        /// Returns a new unique topic. The topic is unique only within the process.
        /// </summary>
        /// <returns></returns>
        object CreateUniqueTopic();

        /// <summary>
        /// Start receiving events.
        /// </summary>
        void Start();

        /// <summary>
        /// Stop receiving events.
        /// </summary>
        void Stop();
    }


    /// <summary>
    /// A process bus backed by a thread.
    /// <seealso cref="IProcessBus"/>
    /// </summary>
    public interface IProcessContext : IProcessBus
    {
        /// <summary>
        /// Wait for underlying thread to complete.
        /// </summary>
        void Join();
        /// <summary>
        /// Wait for underlying thread to complete or for the timeout to expire.
        /// </summary>
        /// <param name="milliseconds"></param>
        /// <returns></returns>
        bool Join(int milliseconds);
    }

    /// <summary>
    /// Fired when a process queue is full.
    /// </summary>
    /// <param name="exception"></param>
    /// <param name="header"></param>
    /// <param name="msg"></param>
    public delegate void OnQueueFull(QueueFullException exception, IMessageHeader header, object msg);
}
