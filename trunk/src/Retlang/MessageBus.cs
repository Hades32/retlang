namespace Retlang
{

    /// <summary>
    /// Message delivery delegate.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="msg"></param>
    public delegate void On<T>(T msg);

    /// <summary>
    /// Retlang message delivery delegate.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="header"></param>
    /// <param name="msg"></param>
    public delegate void OnMessage<T>(IMessageHeader header, T msg);


    /// <summary>
    /// Registry for message subscribers.
    /// </summary>
    public interface ISubscriberRegistry
    {
        /// <summary>
        /// Subscribe to all events.
        /// </summary>
        /// <param name="subscriber"></param>
        void Subscribe(ISubscriber subscriber);
        /// <summary>
        /// Unsubscribe.
        /// </summary>
        /// <param name="subscriber"></param>
        void Unsubscribe(ISubscriber subscriber);
    }

    /// <summary>
    /// Delivers published events to registered subscribers.
    /// </summary>
    public interface IMessageBus : ICommandQueue, ISubscriberRegistry
    {
        /// <summary>
        /// Fired if event is not consumed by any subscriber.
        /// </summary>
        event On<ITransferEnvelope> UnhandledMessageEvent;

        /// <summary>
        /// Publish message to all subscribers.
        /// </summary>
        /// <param name="envelope"></param>
        void Publish(ITransferEnvelope envelope);
    }

    /// <summary>
    /// Default message bus implementation.
    /// </summary>
    public class MessageBus : IMessageBus
    {
        private readonly SubscriberRegistry _subscribers;

        private readonly ICommandQueue _thread;
        private bool _asyncPublish = true;

        /// <summary>
        /// <see cref="IMessageBus.UnhandledMessageEvent"/>
        /// </summary>
        public event On<ITransferEnvelope> UnhandledMessageEvent;

        /// <summary>
        /// Constructs a message bus with the provided backing queue.
        /// </summary>
        /// <param name="thread"></param>
        public MessageBus(ICommandQueue thread)
        {
            _thread = thread;
            _subscribers = new SubscriberRegistry();
        }

        /// <summary>
        /// Toggles whether events are delivered directly to subscribers by publishing thread or the events are queued
        /// and delivered by the message bus thread.
        /// </summary>
        public bool AsyncPublish
        {
            get { return _asyncPublish; }
            set { _asyncPublish = value; }
        }

        /// <summary>
        /// Append command to queue.
        /// </summary>
        /// <param name="command"></param>
        public void Enqueue(Command command)
        {
            _thread.Enqueue(command);
        }

        /// <summary>
        /// Deliver message to any and all interested subscribers.
        /// </summary>
        /// <param name="envelope"></param>
        public void Publish(ITransferEnvelope envelope)
        {
            if (!_asyncPublish)
            {
                if (!_subscribers.Publish(envelope))
                {
                    Command unhandled = delegate
                                            {
                                                On<ITransferEnvelope> env = UnhandledMessageEvent;
                                                if (env != null)
                                                {
                                                    env(envelope);
                                                }
                                            };
                    Enqueue(unhandled);
                }
            }
            else
            {
                Command pubCommand = delegate
                                         {
                                             if (!_subscribers.Publish(envelope))
                                             {
                                                 On<ITransferEnvelope> env = UnhandledMessageEvent;
                                                 if (env != null)
                                                 {
                                                     env(envelope);
                                                 }
                                             }
                                         };
                Enqueue(pubCommand);
            }
        }

        /// <summary>
        /// Subscribe to events.
        /// </summary>
        /// <param name="subscriber"></param>
        public void Subscribe(ISubscriber subscriber)
        {
            _subscribers.Subscribe(subscriber);
        }

        /// <summary>
        /// Unsubscribe
        /// </summary>
        /// <param name="sub"></param>
        public void Unsubscribe(ISubscriber sub)
        {
            _subscribers.Unsubscribe(sub);
        }
    }
}