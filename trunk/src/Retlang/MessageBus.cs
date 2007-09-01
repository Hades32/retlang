using System;
using System.Collections.Generic;
using System.Text;

namespace Retlang
{
    public delegate void OnMessage<T>(IMessageHeader header, T msg); 

    public interface IMessageBus: ICommandQueue, ICommandExceptionHandler, IThreadController
    {     
        void Publish(object topic, object message);
        void Publish(object topic, object message, object replyTopic);

        void Subscribe(ISubscriber subscriber);
        void Unsubscribe(ISubscriber subscriber);
    }
     
    public class MessageBus: IMessageBus
    {
        private readonly List<ISubscriber> _subscribers = new List<ISubscriber>();

        private readonly IProcessThread _thread;
        private readonly CommandQueue _commandQueue;

        public MessageBus()
        {
            _commandQueue = new CommandQueue();
            _thread = new ProcessThread(_commandQueue);
        }

        public void AddExceptionHandler(OnException onExc)
        {
            _commandQueue.ExceptionEvent += onExc;
        }

        public void RemoveExceptionHandler(OnException onExc)
        {
            _commandQueue.ExceptionEvent -= onExc;
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

        public void Enqueue(OnCommand command)
        {
            _thread.Enqueue(command);
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
