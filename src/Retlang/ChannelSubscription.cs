using System;

namespace Retlang
{
    internal class ChannelSubscription<T>
    {
        private readonly ICommandQueue _queue;
        private readonly Action<T> _receive;

        public ChannelSubscription(ICommandQueue queue, Action<T> receive)
        {
            _queue = queue;
            _receive = receive;
        }

        public void OnReceive(T msg)
        {
            Command asyncExec = delegate { _receive(msg); };
            _queue.Enqueue(asyncExec);
        }
    }
}