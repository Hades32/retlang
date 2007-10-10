namespace Retlang
{
    public interface IUnsubscriber
    {
        void Unsubscribe();
    }

    public class Unsubscriber : IUnsubscriber
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