using System.Collections.Generic;

namespace Retlang
{
    public delegate void On<T>(T msg);

    public delegate void OnMessage<T>(IMessageHeader header, T msg);

    public interface IMessageBus : ICommandQueue, IThreadController
    {
        event On<ITransferEnvelope> UnhandledMessageEvent;

        void Publish(ITransferEnvelope envelope);
        void Subscribe(ISubscriber subscriber);
        void Unsubscribe(ISubscriber subscriber);
    }

    public class MessageBus : IMessageBus
    {
        private readonly List<ISubscriber> _subscribers = new List<ISubscriber>();

        private readonly ProcessThread _thread;

        public event On<ITransferEnvelope> UnhandledMessageEvent;

        public MessageBus(ProcessThread thread)
        {
            _thread = thread;
        }

        public void Start()
        {
            _thread.Start();
        }

        public void Stop()
        {
            _thread.Stop();
        }

        public void Join()
        {
            _thread.Join();
        }

        public void Enqueue(Command command)
        {
            _thread.Enqueue(command);
        }

        public void Publish(ITransferEnvelope envelope)
        {
            Command pubCommand = delegate
                                     {
                                         bool published = false;
                                         foreach (ISubscriber sub in _subscribers)
                                         {
                                             if (sub.Receive(envelope))
                                             {
                                                 published = true;
                                             }
                                         }
                                         if (!published)
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
            Command subCommand = delegate { _subscribers.Add(subscriber); };
            Enqueue(subCommand);
        }

        public void Unsubscribe(ISubscriber sub)
        {
            Command unSub = delegate { _subscribers.Remove(sub); };
            Enqueue(unSub);
        }
    }
}