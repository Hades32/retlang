using System;
using System.Collections.Generic;
using System.Text;

namespace Retlang
{
    public interface ISubscriber
    {
        void Receive(IMessageHeader header, object msg);
    }

    public class TopicSubscriber<T>: ISubscriber
    {
        private readonly ITopicMatcher _topic;
        private readonly OnMessage<T> _onMessage;
        private readonly ICommandQueue _queue;

        public TopicSubscriber(ITopicMatcher topic, OnMessage<T> onMessage, ICommandQueue targetQueue)
        {
            _topic = topic;
            _onMessage = onMessage;
            _queue = targetQueue;
        }

        public ITopicMatcher Topic
        {
            get { return _topic; }
        }

        public Type MessageType
        {
            get { return typeof(T); }
        }

        public void Receive(IMessageHeader header, object msg)
        {
            if (!MessageType.IsAssignableFrom(msg.GetType()))
            {
                return;
            }

            T typedMsg = (T)msg;
            if (_topic.Matches(header.Topic))
            {
                OnCommand toExecute = delegate
                {
                    _onMessage(header, typedMsg);
                };
                _queue.Enqueue(toExecute);
            }
        }
    }
}
