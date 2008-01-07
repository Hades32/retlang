using System.Collections.Generic;

namespace Retlang
{
    public interface IProcessBus : IObjectPublisher, ICommandQueue, ICommandTimer
    {
        /// <summary>
        /// Callback from any and all publishing threads. Not Thread Safe.
        /// Will only happen if the max size of the queue and the max wait times are set.
        /// </summary>
        event OnQueueFull QueueFullEvent;

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

        IUnsubscriber Subscribe<T>(ITopicMatcher topic, OnMessage<T> msg);

        IRequestReply<T> SendRequest<T>(ITransferEnvelope env);
        IRequestReply<T> SendRequest<T>(object topic, object msg);
        void SendAsyncRequest<T>(object topic, object msg, OnMessage<T> onReply, Command onTimeout, long requestTimeout);

        object CreateUniqueTopic();

        void Start();
        void Stop();
    }

    public interface IProcessContext : IProcessBus
    {
        void Join();
        bool Join(int milliseconds);
    }

    public delegate void OnQueueFull(QueueFullException exception, IMessageHeader header, object msg);
}
