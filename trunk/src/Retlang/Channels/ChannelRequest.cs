using System.Collections.Generic;
using System.Threading;

namespace Retlang.Channels
{
    internal class ChannelRequest<R, M> : IRequest<R, M>, IReply<M>
    {
        private readonly object _lock = new object();
        private readonly R _req;
        private readonly Queue<M> _resp = new Queue<M>();
        private bool _disposed;

        public ChannelRequest(R req)
        {
            _req = req;
        }

        public R Request
        {
            get { return _req; }
        }

        public bool SendReply(M response)
        {
            lock (_lock)
            {
                if (_disposed)
                {
                    return false;
                }
                _resp.Enqueue(response);
                Monitor.PulseAll(_lock);
                return true;
            }
        }

        public bool Receive(int timeout, out M result)
        {
            lock (_lock)
            {
                if (_resp.Count > 0)
                {
                    result = _resp.Dequeue();
                    return true;
                }
                if (_disposed)
                {
                    result = default(M);
                    return false;
                }
                Monitor.Wait(_lock, timeout);
                if (_resp.Count > 0)
                {
                    result = _resp.Dequeue();
                    return true;
                }
            }
            result = default(M);
            return false;
        }

        /// <summary>
        /// Stop receiving replies.
        /// </summary>
        public void Dispose()
        {
            lock (_lock)
            {
                _disposed = true;
                Monitor.PulseAll(_lock);
            }
        }
    }
}
