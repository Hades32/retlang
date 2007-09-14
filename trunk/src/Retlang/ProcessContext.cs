using System;
using System.Collections.Generic;
using System.Text;

namespace Retlang
{
    public interface IProcessContext: ICommandTimer, ICommandQueue, IThreadController, ICommandExceptionHandler, IObjectPublisher
    {
        void Publish(ITransferEnvelope toPublish);

        IUnsubscriber SubscribeToKeyedBatch<K, V>(ITopicMatcher topic, ResolveKey<K, V> keyResolver, On<IDictionary<K, IMessageEnvelope<V>>> target, int minBatchIntervalInMs);
        IUnsubscriber SubscribeToBatch<T>(ITopicMatcher topic, On<IList<IMessageEnvelope<T>>> msg, int minBatchIntervalInMs);
        IUnsubscriber Subscribe<T>(ITopicMatcher topic, OnMessage<T> msg);

        IRequestReply<T> SendRequest<T>(ITransferEnvelope env);
        IRequestReply<T> SendRequest<T>(object topic, object msg);

        object CreateUniqueTopic();
    }

    public class ProcessContext: IProcessContext
    {
        private ITransferEnvelopeFactory _envelopeFactory;
        private readonly IMessageBus _bus;
        private readonly IProcessThread _processThread;

        public ProcessContext(IMessageBus messageBus, IProcessThread runner, ITransferEnvelopeFactory factory )
        {
            _bus = messageBus;
            _processThread = runner;
            _envelopeFactory = factory;
        }

        public ITransferEnvelopeFactory TransferEnvelopeFactory
        {
            get { return _envelopeFactory; }
            set { _envelopeFactory = value; }
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


        public void Schedule(Command command, int intervalInMs)
        {
            _processThread.Schedule(command, intervalInMs);
        }

        public void ScheduleOnInterval(Command command, int firstIntervalInMs, int regularIntervalInMs)
        {
            _processThread.ScheduleOnInterval(command, firstIntervalInMs, regularIntervalInMs);
        }

        public void Enqueue(Command command)
        {
            _processThread.Enqueue(command);
        }

        public void Publish(object topic, object msg, object replyToTopic)
        {
            Publish(_envelopeFactory.Create(topic, msg, replyToTopic));
        }

        public void Publish(ITransferEnvelope toPublish)
        {
            _bus.Publish(toPublish);
        }

        public void Publish(object topic, object msg)
        {
            Publish(topic, msg, null);
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

        public object CreateUniqueTopic()
        {
            return new object();
        }

        public IRequestReply<T> SendRequest<T>(ITransferEnvelope env)
        {
            object requestTopic = env.Header.ReplyTo;
            TopicRequestReply<T> req = new TopicRequestReply<T>();
            TopicSubscriber<T> subscriber = new TopicSubscriber<T>(new TopicEquals(requestTopic), req.OnReply, _bus);
            _bus.Subscribe(subscriber);
            req.Unsubscriber = new Unsubscriber(subscriber, _bus);
            _bus.Publish(env);
            return req;
        }


        public IRequestReply<T> SendRequest<T>(object topic, object msg)
        {
            return SendRequest<T>(_envelopeFactory.Create(topic, msg, CreateUniqueTopic()));
        }

    }
}
