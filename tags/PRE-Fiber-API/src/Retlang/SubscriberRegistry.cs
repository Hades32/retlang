namespace Retlang
{
    /// <summary>
    /// Delegate for message delivery.
    /// </summary>
    /// <param name="envelope"></param>
    /// <param name="received"></param>
    public delegate void OnReceive(ITransferEnvelope envelope, ref bool received);

    internal class SubscriberRegistry : ISubscriberRegistry
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