using System;
using System.Collections.Generic;
using System.Text;

namespace Retlang
{
    public interface ISubscriber
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="envelope"></param>
        /// <returns>true if message is processed. false if ignored.</returns>
        bool Receive(ITransferEnvelope envelope);
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

        public bool Receive(ITransferEnvelope envelope)
        {
            if (!MessageType.IsAssignableFrom(envelope.MessageType))
            {
                return false;
            }
      
            if (_topic.Matches(envelope.Header.Topic))
            {
                T typedMsg = (T)envelope.ResolveMessage();
                Command toExecute = delegate
                {
                    _onMessage(envelope.Header, typedMsg);
                };
                _queue.Enqueue(toExecute);
                return true;
            }
            return false;
        }
    }
}
