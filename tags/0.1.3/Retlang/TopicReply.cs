using System;
using System.Collections.Generic;
using System.Threading;

namespace Retlang
{
    public interface IReply<T>
    {
        T Message { get; }
        IMessageHeader Header { get; }
    }

    public class Reply<T> : IReply<T>
    {
        private readonly IMessageHeader _header;
        private readonly T _message;

        public Reply(IMessageHeader header, T message)
        {
            _header = header;
            _message = message;
        }

        public T Message
        {
            get { return _message; }
        }

        public IMessageHeader Header
        {
            get { return _header; }
        }
    }

    public interface IRequestReply<T>
    {
        IReply<T> Receive(int waitTimeoutInMs);
    }

    public class TopicRequestReply<T>: IRequestReply<T>
    {
        private object _lock = new object();

        private bool _timedOut;
        private IReply<T> _reply;
        private IUnsubscriber _unsub;

        public IUnsubscriber Unsubscriber
        {
            set { _unsub = value; }
        }

        public void OnReply(IMessageHeader header, T msg)
        {
            lock (_lock)
            {
                _reply = new Reply<T>(header, msg);
                _unsub.Unsubscribe();
                Monitor.PulseAll(_lock);
            }
        }

        public IReply<T> Receive(int timeoutInMs)
        {
            lock(_lock)
            {
                if (_timedOut)
                {
                    return _reply;
                }

                if (_reply == null)
                {
                    Monitor.Wait(_lock, timeoutInMs);
                }
                _timedOut = true;
                _unsub.Unsubscribe();
                return _reply;
            }
        }
    }
}
