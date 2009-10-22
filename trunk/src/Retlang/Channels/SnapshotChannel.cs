using System;
using Retlang.Core;

namespace Retlang.Channels
{
    public class SnapshotChannel<T> : ISnapshotChannel<T>
    {
        private readonly int _timeoutInMs;
        private readonly IChannel<T> _updatesChannel = new Channel<T>();
        private readonly RequestReplyChannel<object, T> _requestChannel = new RequestReplyChannel<object, T>();

        public SnapshotChannel(int timeoutInMs)
        {
            _timeoutInMs = timeoutInMs;
        }

        public void PrimedSubscribe(IDisposingExecutor fiber, Action<T> handler)
        {
            using (var reply = _requestChannel.SendRequest(new object()))
            {
                if (reply == null)
                {
                    throw new ArgumentException(typeof (T).Name + " synchronous request has no reply subscriber.");
                }

                T result;
                if (!reply.Receive(_timeoutInMs, out result))
                {
                    throw new ArgumentException(typeof (T).Name + " synchronous request timed out in " + _timeoutInMs);
                }

                handler(result);

                _updatesChannel.Subscribe(fiber, handler);
            }
        }

        public void Publish(T update)
        {
            _updatesChannel.Publish(update);
        }

        public void ReplyToPrimingRequest(IDisposingExecutor fiber, Func<T> getter)
        {
            _requestChannel.Subscribe(fiber, request => request.SendReply(getter()));
        }
    }
}