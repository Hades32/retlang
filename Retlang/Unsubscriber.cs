using System;
using System.Collections.Generic;
using System.Text;

namespace Retlang
{
    public interface IUnsubscriber
    {
        void Unsubscribe();
    }

    public class Unsubscriber: IUnsubscriber
    {
        private readonly IMessageBus _bus;
        private readonly ISubscriber _sub;
        public Unsubscriber(ISubscriber sub, IMessageBus bus)
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
