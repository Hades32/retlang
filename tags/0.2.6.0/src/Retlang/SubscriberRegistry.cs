namespace Retlang
{
    public delegate void OnReceive(ITransferEnvelope envelope, ref bool received);

    public class SubscriberRegistry : ISubscriberRegistry
    {
        private event OnReceive ReceiveEvent;

        public bool Publish(ITransferEnvelope envelope)
        {
            bool published = false;
            OnReceive rcv = ReceiveEvent;
            if (rcv != null)
            {
                rcv(envelope, ref published);
            }
            return published;
        }

        public void Subscribe(ISubscriber subscriber)
        {
            ReceiveEvent += subscriber.Receive;
        }

        public void Unsubscribe(ISubscriber sub)
        {
            ReceiveEvent -= sub.Receive;
        }
    }
}