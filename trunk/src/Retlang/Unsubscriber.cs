namespace Retlang
{
    /// <summary>
    /// Unsubscribe controller.
    /// </summary>
    public interface IUnsubscriber
    {
        /// <summary>
        /// Unsubscribe.
        /// </summary>
        void Unsubscribe();
    }

    internal class Unsubscriber : IUnsubscriber
    {
        private readonly ISubscriberRegistry _bus;
        private readonly ISubscriber _sub;

        public Unsubscriber(ISubscriber sub, ISubscriberRegistry bus)
        {
            _sub = sub;
            _bus = bus;
        }

        public void Unsubscribe()
        {
            _bus.Unsubscribe(_sub);
        }
    }
}