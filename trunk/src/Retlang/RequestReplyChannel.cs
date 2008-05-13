using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Retlang
{
    /// <summary>
    /// Channel for synchronous and asynchronous requests.
    /// </summary>
    /// <typeparam name="R"></typeparam>
    /// <typeparam name="M"></typeparam>
    public class RequestReplyChannel<R,M>
    {
        private readonly Channel<IChannelRequest<R,M>> _requestChannel = new Channel<IChannelRequest<R,M>>();

        /// <summary>
        /// Subscribe to requests.
        /// </summary>
        /// <param name="responder"></param>
        /// <param name="onRequest"></param>
        /// <returns></returns>
        public IUnsubscriber Subscribe(IProcessBus responder, Action<IChannelRequest<R, M>> onRequest)
        {
            return _requestChannel.Subscribe(responder, onRequest);
        }

        /// <summary>
        /// Send request to any and all subscribers.
        /// </summary>
        /// <param name="p"></param>
        /// <returns>null if no subscribers registered for request.</returns>
        public IChannelReply<M> SendRequest(R p)
        {
            ChannelRequest<R, M> request = new ChannelRequest<R, M>(p);
            if (_requestChannel.Publish(request))
                return request;
            return null;
        }
    }

    /// <summary>
    /// A request object that can be used to send 1 or many responses to the initial request.
    /// </summary>
    /// <typeparam name="R"></typeparam>
    /// <typeparam name="M"></typeparam>
    public interface IChannelRequest<R, M>
    {
        /// <summary>
        /// Send one or more responses.
        /// </summary>
        /// <param name="replyMsg"></param>
        /// <returns></returns>
        bool SendReply(M replyMsg);
    }

    internal class ChannelRequest<R, M>: IChannelRequest<R,M>, IChannelReply<M>
    {
        private readonly object _lock = new object();
        private readonly R _req;
        private readonly Queue<M> _resp = new Queue<M>();
        private bool _disposed;

        public ChannelRequest(R req)
        {
            _req = req;
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
                if(_disposed)
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

    /// <summary>
    /// Used to receive one or more replies.
    /// </summary>
    /// <typeparam name="M"></typeparam>
    public interface IChannelReply<M>: IDisposable
    {
        /// <summary>
        /// Receive a single response. Can be called repeatedly for multiple replies.
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        bool Receive(int timeout, out M result);
    }
}
