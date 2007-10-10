using System.Collections.Generic;

namespace Retlang
{
    public delegate void On<T>(T msg);

    public delegate void OnMessage<T>(IMessageHeader header, T msg);

    public interface ISubscriberRegistry
    {
        void Subscribe(ISubscriber subscriber);
        void Unsubscribe(ISubscriber subscriber);        
    }

    public interface IMessageBus : ICommandQueue, ISubscriberRegistry
    {
        event On<ITransferEnvelope> UnhandledMessageEvent;

        void Publish(ITransferEnvelope envelope);
    }

    public class MessageBus : IMessageBus
    {
        private readonly SubscriberRegistry _subscribers;

        private readonly ICommandQueue _thread;

        public event On<ITransferEnvelope> UnhandledMessageEvent;

        public MessageBus(ICommandQueue thread)
        {
            _thread = thread;
            _subscribers = new SubscriberRegistry(thread);
        }

        public void Enqueue(Command command)
        {
            _thread.Enqueue(command);
        }

        public void Publish(ITransferEnvelope envelope)
        {
            Command pubCommand = delegate
                                     {
                                         if(!_subscribers.Publish(envelope))
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

        public void Subscribe(ISubscriber subscriber)
        {
            _subscribers.Subscribe(subscriber); 
        }

        public void Unsubscribe(ISubscriber sub)
        {
            _subscribers.Unsubscribe(sub);
        }
    }
}