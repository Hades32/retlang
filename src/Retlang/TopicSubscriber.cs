using System;
using System.Collections.Generic;
using System.Text;

namespace Retlang
{
    public interface ISubscriber
    {
        void Receive(ITransferEnvelope envelope);
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

        public void Receive(ITransferEnvelope envelope)
        {
            if (!MessageType.IsAssignableFrom(envelope.MessageType))
            {
                return;
            }
      
            if (_topic.Matches(envelope.Header.Topic))
            {
                T typedMsg = (T)envelope.ResolveMessage;
                OnCommand toExecute = delegate
                {
                    _onMessage(envelope.Header, typedMsg);
                };
                _queue.Enqueue(toExecute);
            }
        }
    }
}
