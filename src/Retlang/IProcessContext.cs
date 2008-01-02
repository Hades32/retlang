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

        IUnsubscriber SubscribeToKeyedBatch<K, V>(ITopicMatcher topic, ResolveKey<K, V> keyResolver,
                                                  On<IDictionary<K, IMessageEnvelope<V>>> target,
                                                  int minBatchIntervalInMs);

        IUnsubscriber SubscribeToBatch<T>(ITopicMatcher topic, On<IList<IMessageEnvelope<T>>> msg,
                                          int minBatchIntervalInMs);

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