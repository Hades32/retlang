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

        public IUnsubscriber Subscribe(IProcessBus responder, Action<IChannelRequest<R, M>> onRequest)
        {
            return _requestChannel.Subscribe(responder, onRequest);
        }

        public IChannelResponse<M> SendRequest(R p)
        {
            ChannelRequest<R, M> request = new ChannelRequest<R, M>(p);
            _requestChannel.Publish(request);
            return request;
        }
    }

    public interface IChannelRequest<R, M>
    {
        bool SendResponse(M dateTime);
    }

    internal class ChannelRequest<R, M>: IChannelRequest<R,M>, IChannelResponse<M>
    {
        private readonly object _lock = new object();
        private readonly R _req;
        private List<M> _resp = new List<M>();
        private bool _disposed;

        public ChannelRequest(R req)
        {
            _req = req;
        }

        public bool SendResponse(M response)
        {
            lock (_lock)
            {
                if (_disposed)
                {
                    return false;
                }
                _resp.Add(response);
                Monitor.PulseAll(_lock);
                return true;
            }
        }

        public bool Receive(int timeout, out M result)
        {
            lock (_lock)
            {
                if (_resp != null && _resp.Count > 0)
                {
                    result = _resp[0];
                    _resp.RemoveAt(0);
                    return true;
                }
                Monitor.Wait(_lock, timeout);
                if (_resp != null && _resp.Count > 0)
                {
                    result = _resp[0];
                    _resp.RemoveAt(0);
                    return true;
                }
            }
            result = default(M);
            return false;
        }

        public void Dispose()
        {
            lock (_lock)
            {
                _disposed = true;
                Monitor.PulseAll(_lock);
            }
        }
    }

    public interface IChannelResponse<M>: IDisposable
    {
        bool Receive(int timeout, out M result);
    }
}
