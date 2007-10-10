using System.Collections.Generic;

namespace Retlang
{
    public class SubscriberRegistry: ISubscriberRegistry
    {
        private readonly ICommandQueue _targetThread;
        private readonly List<ISubscriber> _subscribers = new List<ISubscriber>();

        public SubscriberRegistry(ICommandQueue targetThread)
        {
            _targetThread = targetThread;
        }

        public bool Publish(ITransferEnvelope envelope)
        {
            bool published = false;
            foreach (ISubscriber sub in _subscribers)
            {
                if (sub.Receive(envelope))
                {
                    published = true;
                }
            }
            return published;
        }

        public void Subscribe(ISubscriber subscriber)
        {
            Command subCommand = delegate { _subscribers.Add(subscriber); };
            _targetThread.Enqueue(subCommand);
        }

        public void Unsubscribe(ISubscriber sub)
        {
            Command unSub = delegate { _subscribers.Remove(sub); };
            _targetThread.Enqueue(unSub);
        }
    }
}
