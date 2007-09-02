using System;
using System.Collections.Generic;
using System.Text;

namespace Retlang
{
    public interface IProcessContext: ICommandTimer, ICommandQueue, IThreadController, ICommandExceptionHandler, IObjectPublisher
    {
        IUnsubscriber SubscribeToKeyedBatch<K, V>(ITopicMatcher topic, ResolveKey<K, V> keyResolver, On<IDictionary<K, IMessageEnvelope<V>>> target, int minBatchIntervalInMs);
        IUnsubscriber SubscribeToBatch<T>(ITopicMatcher topic, On<IList<IMessageEnvelope<T>>> msg, int minBatchIntervalInMs);
        IUnsubscriber Subscribe<T>(ITopicMatcher topic, OnMessage<T> msg);
        IRequestReply<T> SendRequest<T>(object topic, object msg);
    }

    public class ProcessContext: IProcessContext
    {
        private readonly IMessageBus _bus;
        private readonly IProcessThread _processThread;

        public ProcessContext(IMessageBus messageBus, IProcessThread runner )
        {
            _bus = messageBus;
            _processThread = runner;
        }

        public void Start()
        {
            _processThread.Start();
        }

        public void Stop()
        {
            _processThread.Stop();
        }

        public void Join()
        {
            _processThread.Join();
        }

        public void AddExceptionHandler(OnException onExc)
        {
            _processThread.AddExceptionHandler(onExc);
        }

        public void RemoveExceptionHandler(OnException onExc)
        {
            _processThread.RemoveExceptionHandler(onExc);
        }


        public void Schedule(OnCommand command, int intervalInMs)
        {
            _processThread.Schedule(command, intervalInMs);
        }

        public void ScheduleOnInterval(OnCommand command, int firstIntervalInMs, int regularIntervalInMs)
        {
            _processThread.ScheduleOnInterval(command, firstIntervalInMs, regularIntervalInMs);
        }

        public void Enqueue(OnCommand command)
        {
            _processThread.Enqueue(command);
        }

        public void Publish(object topic, object msg, object replyToTopic)
        {
            _bus.Publish(topic, msg, replyToTopic);
        }

        public void Publish(object topic, object msg)
        {
            _bus.Publish(topic, msg);
        }

        public IUnsubscriber SubscribeToKeyedBatch<K,V>(ITopicMatcher topic, ResolveKey<K,V> keyResolver, On<IDictionary<K,IMessageEnvelope<V>>> target, int minBatchIntervalInMs)
        {
            KeyedBatchSubscriber<K, V> batch = new KeyedBatchSubscriber<K, V>(keyResolver, target, this, minBatchIntervalInMs);
            return Subscribe<V>(topic, batch.ReceiveMessage);
        }

        public IUnsubscriber SubscribeToBatch<T>(ITopicMatcher topic, On<IList<IMessageEnvelope<T>>> msg, int minBatchIntervalInMs)
        {
            BatchSubscriber<T> batch = new BatchSubscriber<T>(msg, this, minBatchIntervalInMs);
            return Subscribe<T>(topic, batch.ReceiveMessage);
        }

        public IUnsubscriber Subscribe<T>(ITopicMatcher topic, OnMessage<T> msg)
        {
            TopicSubscriber<T> subscriber = new TopicSubscriber<T>(topic, msg, _processThread);
            _bus.Subscribe(subscriber);
            return new Unsubscriber(subscriber, _bus);
        }

        public IRequestReply<T> SendRequest<T>(object topic, object msg)
        {
            object requestTopic = new object();
            TopicRequestReply<T> req = new TopicRequestReply<T>();
            TopicSubscriber<T> subscriber = new TopicSubscriber<T>(new TopicMatcher(requestTopic), req.OnReply, _bus);
            _bus.Subscribe(subscriber);
            req.Unsubscriber = new Unsubscriber(subscriber, _bus);
            _bus.Publish(topic, msg, requestTopic);
            return req;
        }

    }
}
