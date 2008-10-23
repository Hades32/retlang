using System;
using Retlang.Core;

namespace Retlang.Channels
{
    internal class Unsubscriber<T> : IUnsubscriber
    {
        private readonly Action<T> _receiveMethod;
        private readonly Channel<T> _channel;

        public Unsubscriber(Action<T> receiveMethod, Channel<T> channel)
        {
            _receiveMethod = receiveMethod;
            _channel = channel;
        }

        public void Dispose()
        {
            _channel.Unsubscribe(_receiveMethod);
        }
    }
}
