using System;
using System.Collections.Generic;
using System.Text;

namespace Retlang
{
    public delegate void OnMessage<T>(IMessageHeader header, T msg); 

    public interface IMessageBus
    {
        IProcessThread Thread { get; }

        void Publish(object topic, object message);
        void Publish(object topic, object message, object replyTopic);

        void Subscribe(ISubscriber subscriber);
        void Unsubscribe(ISubscriber subscriber);

    }

    public class MessageBus: IMessageBus
    {
        private readonly List<ISubscriber> _subscribers = new List<ISubscriber>();

        private readonly IProcessThread _thread;

        public MessageBus()
        {
            _thread = new ProcessThread(new CommandQueue());
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

        public IProcessThread Thread
        {
            get { return _thread; }
        }

        public void Publish(object topic, object message, object replyTo)
        {
            IMessageHeader header = new MessageHeader(topic, replyTo);
            lock (_subscribers)
            {
                foreach (ISubscriber sub in _subscribers)
                {
                    sub.Receive(header, message);
                }
            }
        }

        public void Publish(object topic, object message)
        {
            Publish(topic, message, null);
        }

        public void Subscribe(ISubscriber subscriber)
        {
            lock (_subscribers)
            {
                _subscribers.Add(subscriber);
            }
        }

        public void Unsubscribe(ISubscriber sub)
        {
            lock (_subscribers)
            {
                _subscribers.Remove(sub);
            }
        }


    }
}
