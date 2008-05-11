using System.Collections.Generic;

namespace Retlang
{
    /// <summary>
    /// Base class for thread and pool backed process bus instances.
    /// </summary>
    public class ProcessBus : IProcessBus, ISubscriber
    {
        /// <summary>
        /// Fired when queue is full.
        /// </summary>
        public event OnQueueFull QueueFullEvent;

        private readonly ITransferEnvelopeFactory _envelopeFactory;
        private readonly IMessageBus _bus;
        private readonly IProcessQueue _processThread;
        private readonly SubscriberRegistry _subscribers;

        /// <summary>
        /// construct new instance.
        /// </summary>
        /// <param name="messageBus"></param>
        /// <param name="runner"></param>
        /// <param name="factory"></param>
        public ProcessBus(IMessageBus messageBus, IProcessQueue runner, ITransferEnvelopeFactory factory)
        {
            _bus = messageBus;
            _processThread = runner;
            _envelopeFactory = factory;
            _subscribers = new SubscriberRegistry();
        }

        internal IProcessQueue ProcessQueue
        {
            get { return _processThread; }
        }

        /// <summary>
        /// Start receiving events.
        /// </summary>
        public void Start()
        {
            _processThread.Start();
            _bus.Subscribe(this);
        }

        /// <summary>
        /// Stop receiving events.
        /// </summary>
        public void Stop()
        {
            _processThread.Stop();
            _bus.Unsubscribe(this);
        }

        /// <summary>
        /// <see cref="ICommandTimer.Schedule(Command,long)"/>
        /// </summary>
        /// <param name="command"></param>
        /// <param name="intervalInMs"></param>
        /// <returns></returns>
        public ITimerControl Schedule(Command command, long intervalInMs)
        {
            return _processThread.Schedule(command, intervalInMs);
        }

        /// <summary>
        /// <see cref="ICommandTimer.ScheduleOnInterval(Command,long,long)"/>
        /// </summary>
        /// <param name="command"></param>
        /// <param name="firstIntervalInMs"></param>
        /// <param name="regularIntervalInMs"></param>
        /// <returns></returns>
        public ITimerControl ScheduleOnInterval(Command command, long firstIntervalInMs, long regularIntervalInMs)
        {
            return _processThread.ScheduleOnInterval(command, firstIntervalInMs, regularIntervalInMs);
        }

        /// <summary>
        /// <see cref="ICommandQueue.Enqueue(Command)"/>
        /// </summary>
        /// <param name="command"></param>
        public void Enqueue(Command command)
        {
            _processThread.Enqueue(command);
        }

        /// <summary>
        /// <see cref="IObjectPublisher.Publish(object,object,object)"/>
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="msg"></param>
        /// <param name="replyToTopic"></param>
        public void Publish(object topic, object msg, object replyToTopic)
        {
            Publish(_envelopeFactory.Create(topic, msg, replyToTopic));
        }

        /// <summary>
        /// Publish the wrapped message.
        /// </summary>
        /// <param name="toPublish"></param>
        public void Publish(ITransferEnvelope toPublish)
        {
            _bus.Publish(toPublish);
        }

        /// <summary>
        /// <see cref="IObjectPublisher.Publish(object,object)"/>
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="msg"></param>
        public void Publish(object topic, object msg)
        {
            Publish(topic, msg, null);
        }

        /// <summary>
        /// Posts the message directly to the process bus. The message is not broadcast.
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="msg"></param>
        /// <param name="replyToTopic"></param>
        /// <returns></returns>
        public bool Post(object topic, object msg, object replyToTopic)
        {
            ITransferEnvelope env = _envelopeFactory.Create(topic, msg, replyToTopic);
            return _subscribers.Publish(env);
        }

        /// <summary>
        /// Subscribe to batched events. The key resolver allows duplicates to be dropped if events arrive faster
        /// than the bus can consume the events.
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="topic"></param>
        /// <param name="keyResolver"></param>
        /// <param name="target"></param>
        /// <param name="minBatchIntervalInMs"></param>
        /// <returns></returns>
        public IUnsubscriber SubscribeToKeyedBatch<K, V>(ITopicMatcher topic, ResolveKey<K, V> keyResolver,
                                                         On<IDictionary<K, IMessageEnvelope<V>>> target,
                                                         int minBatchIntervalInMs)
        {
            KeyedBatchSubscriber<K, V> batch =
                new KeyedBatchSubscriber<K, V>(keyResolver, target, this, minBatchIntervalInMs);
            return CreateSubscription<V>(topic, batch.ReceiveMessage);
        }

        /// <summary>
        /// A batched subscription. An interval can be specified for batching.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="topic"></param>
        /// <param name="msg"></param>
        /// <param name="minBatchIntervalInMs"></param>
        /// <returns></returns>
        public IUnsubscriber SubscribeToBatch<T>(ITopicMatcher topic, On<IList<IMessageEnvelope<T>>> msg,
                                                 int minBatchIntervalInMs)
        {
            BatchSubscriber<T> batch = new BatchSubscriber<T>(msg, this, minBatchIntervalInMs);
            return CreateSubscription<T>(topic, batch.ReceiveMessage);
        }

        /// <summary>
        /// Subscribes to the last event. Stale events are dropped in favor of newer events. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="topic"></param>
        /// <param name="msg"></param>
        /// <param name="minBatchIntervalInMs"></param>
        /// <returns></returns>
        public IUnsubscriber SubscribeToLast<T>(ITopicMatcher topic, OnMessage<T> msg, int minBatchIntervalInMs)
        {
            LastSubscriber<T> last = new LastSubscriber<T>(msg, this, minBatchIntervalInMs);
            return CreateSubscription<T>(topic, last.ReceiveMessage);
        }

        /// <summary>
        /// Subscribe to all events matching the generic type and topic matcher.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="topic"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public IUnsubscriber Subscribe<T>(ITopicMatcher topic, OnMessage<T> msg)
        {
            OnMessage<T> asyncReceive = CreateReceiveOnProcessThread(msg);
            return CreateSubscription(topic, asyncReceive);
        }

        private IUnsubscriber CreateSubscription<T>(ITopicMatcher topic, OnMessage<T> asyncReceive)
        {
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

        /// <summary>
        /// Creates a unique topic for this process.
        /// </summary>
        /// <returns></returns>
        public object CreateUniqueTopic()
        {
            return new object();
        }

        /// <summary>
        /// <see cref="IProcessBus.SendRequest{T}(ITransferEnvelope)"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="env"></param>
        /// <returns></returns>
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

        /// <summary>
        /// <see cref="IProcessBus.SendAsyncRequest{T}(object,object,OnMessage{T},Command,long)"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="topic"></param>
        /// <param name="msg"></param>
        /// <param name="onReply"></param>
        /// <param name="onTimeout"></param>
        /// <param name="requestTimeout"></param>
        public void SendAsyncRequest<T>(object topic, object msg, OnMessage<T> onReply, Command onTimeout, long requestTimeout)
        {
            AsyncRequestSubscriber<T> sub = new AsyncRequestSubscriber<T>(onReply, onTimeout);
            object replyTopic = CreateUniqueTopic();
            IUnsubscriber replySubscriber = Subscribe<T>(new TopicEquals(replyTopic), sub.OnReceive);
            sub.Unsubscriber = replySubscriber;
            if (onTimeout != null)
            {
                ITimerControl timeoutControl = Schedule(sub.OnTimeout, requestTimeout);
                sub.TimeoutControl = timeoutControl;
            }
            Publish(topic, msg, replyTopic);
        }

        /// <summary>
        /// <see cref="IProcessBus.SendRequest{T}(object,object)"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="topic"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public IRequestReply<T> SendRequest<T>(object topic, object msg)
        {
            return SendRequest<T>(_envelopeFactory.Create(topic, msg, CreateUniqueTopic()));
        }

        /// <summary>
        /// Receives event.
        /// </summary>
        /// <param name="envelope"></param>
        /// <param name="consumed"></param>
        public void Receive(ITransferEnvelope envelope, ref bool consumed)
        {
            if (_subscribers.Publish(envelope))
            {
                consumed = true;
            }
        }
    }
}
