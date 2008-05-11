using System.Threading;

namespace Retlang
{
    /// <summary>
    /// Response to Request.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReply<T>
    {
        /// <summary>
        /// The body of the reply
        /// </summary>
        T Message { get; }
        /// <summary>
        /// Reply Header.
        /// </summary>
        IMessageHeader Header { get; }
    }

    internal class Reply<T> : IReply<T>
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

    /// <summary>
    /// Blocking interface for a request.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRequestReply<T>
    {
        /// <summary>
        /// Returns immediately if reply has already been received. Blocks waiting for reply or timeout.
        /// </summary>
        /// <param name="waitTimeoutInMs"></param>
        /// <returns></returns>
        IReply<T> Receive(int waitTimeoutInMs);
    }

    internal class TopicRequestReply<T> : IRequestReply<T>
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
            lock (_lock)
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