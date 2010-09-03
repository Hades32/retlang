using System;
using Retlang.Core;

namespace Retlang.Channels
{
    internal class Unsubscriber<T> : IUnsubscriber
    {
        private readonly Action<T> _receiver;
        private readonly Channel<T> _channel;
        private readonly ISubscriptions _subscriptions;

        public Unsubscriber(Action<T> receiver, Channel<T> channel, ISubscriptions subscriptions)
        {
            _receiver = receiver;
            _channel = channel;
            _subscriptions = subscriptions;
        }

        public void Dispose()
        {
            _channel.Unsubscribe(_receiver);
            _subscriptions.Deregister(this);
        }
    }
}
