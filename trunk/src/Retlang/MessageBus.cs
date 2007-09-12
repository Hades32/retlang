using System;
using System.Collections.Generic;
using System.Text;

namespace Retlang
{
    public delegate void On<T>(T msg);
    public delegate void OnMessage<T>(IMessageHeader header, T msg);
  
    public interface IMessageBus: ICommandQueue, ICommandExceptionHandler, IThreadController
    {
        event On<ITransferEnvelope> UnhandledMessageEvent;

        void Publish(ITransferEnvelope envelope);
        void Subscribe(ISubscriber subscriber);
        void Unsubscribe(ISubscriber subscriber);
    }
     
    public class MessageBus: IMessageBus
    {
        private readonly List<ISubscriber> _subscribers = new List<ISubscriber>();

        private readonly IProcessThread _thread;
        private readonly CommandQueue _commandQueue;

        public event On<ITransferEnvelope> UnhandledMessageEvent;

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

        public void Publish(ITransferEnvelope envelope)
        {
            OnCommand pubCommand = delegate
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
            OnCommand subCommand = delegate
            {
                _subscribers.Add(subscriber);
            };
            Enqueue(subCommand);
        }

        public void Unsubscribe(ISubscriber sub)
        {
            OnCommand unSub = delegate
            {
                _subscribers.Remove(sub);
            };
            Enqueue(unSub);
        }


    }
}
