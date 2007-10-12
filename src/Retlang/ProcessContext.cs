using System.Collections.Generic;

namespace Retlang
{
    public interface IProcessBus: IObjectPublisher, ICommandQueue, ICommandTimer
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

        object CreateUniqueTopic();
    }

    public interface IProcessContext : IThreadController, IProcessBus
    {
    }

    public delegate void OnQueueFull(QueueFullException exception, IMessageHeader header, object msg);

    public class ProcessContext : IProcessContext, ISubscriber
    {
        public event OnQueueFull QueueFullEvent;

        private ITransferEnvelopeFactory _envelopeFactory;
        private readonly IMessageBus _bus;
        private readonly IProcessThread _processThread;
        private readonly SubscriberRegistry _subscribers;

        public ProcessContext(IMessageBus messageBus, IProcessThread runner, ITransferEnvelopeFactory factory)
        {
            _bus = messageBus;
            _processThread = runner;
            _envelopeFactory = factory;
            _subscribers = new SubscriberRegistry();
            _bus.Subscribe(this);
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
            _bus.Unsubscribe(this);
        }

        public void Join()
        {
            _processThread.Join();
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

        public IUnsubscriber SubscribeToKeyedBatch<K, V>(ITopicMatcher topic, ResolveKey<K, V> keyResolver,
                                                         On<IDictionary<K, IMessageEnvelope<V>>> target,
                                                         int minBatchIntervalInMs)
        {
            KeyedBatchSubscriber<K, V> batch =
                new KeyedBatchSubscriber<K, V>(keyResolver, target, this, minBatchIntervalInMs);
            return Subscribe<V>(topic, batch.ReceiveMessage);
        }

        public IUnsubscriber SubscribeToBatch<T>(ITopicMatcher topic, On<IList<IMessageEnvelope<T>>> msg,
                                                 int minBatchIntervalInMs)
        {
            BatchSubscriber<T> batch = new BatchSubscriber<T>(msg, this, minBatchIntervalInMs);
            return Subscribe<T>(topic, batch.ReceiveMessage);
        }

        public IUnsubscriber Subscribe<T>(ITopicMatcher topic, OnMessage<T> msg)
        {
            OnMessage<T> asyncReceive = CreateReceiveOnProcessThread(msg);
            TopicSubscriber<T> subscriber = new TopicSubscriber<T>(topic, asyncReceive);
            AddSubscription(subscriber);
            return new Unsubscriber(subscriber, _subscribers);
        }

        private OnMessage<T> CreateReceiveOnProcessThread<T>(OnMessage<T> msg)
        {
            // message received on message bus thread, then executed on process thread.
            OnMessage<T> onMsgBusThread = delegate(IMessageHeader header, T data)
                                              {
                                                  Command toExecute = delegate { msg(header, data); };
                                                  try
                                                  {
                                                      Enqueue(toExecute);
                                                  }
                                                  catch (QueueFullException full)
                                                  {
                                                      OnQueueFull(full, header, data);
                                                  }
                                              };
            return onMsgBusThread;
        }

        private void OnQueueFull(QueueFullException full, IMessageHeader header, object data)
        {
            OnQueueFull onExc = QueueFullEvent;
            if (onExc != null)
            {
                onExc(full, header, data);
            }
        }

        private void AddSubscription(ISubscriber subscriber)
        {
            _subscribers.Subscribe(subscriber);
        }

        public object CreateUniqueTopic()
        {
            return new object();
        }

        public IRequestReply<T> SendRequest<T>(ITransferEnvelope env)
        {
            object requestTopic = env.Header.ReplyTo;
            TopicRequestReply<T> req = new TopicRequestReply<T>();
            TopicSubscriber<T> subscriber = new TopicSubscriber<T>(new TopicEquals(requestTopic), req.OnReply);
            AddSubscription(subscriber);
            req.Unsubscriber = new Unsubscriber(subscriber, _subscribers);
            _bus.Publish(env);
            return req;
        }


        public IRequestReply<T> SendRequest<T>(object topic, object msg)
        {
            return SendRequest<T>(_envelopeFactory.Create(topic, msg, CreateUniqueTopic()));
        }

        public void Receive(ITransferEnvelope envelope, ref bool consumed)
        {
            if( _subscribers.Publish(envelope))
            {
                consumed = true;
            }
        }
    }
}