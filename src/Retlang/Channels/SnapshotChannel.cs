using System;
using Retlang.Fibers;

namespace Retlang.Channels
{
    ///<summary>
    /// A SnapshotChannel is a channel that allows for the transmission of an initial snapshot followed by incremental updates.
    /// The class is thread safe.
    ///</summary>
    ///<typeparam name="T"></typeparam>
    public class SnapshotChannel<T> : ISnapshotChannel<T>
    {
        private readonly int _timeoutInMs;
        private readonly IChannel<T> _updatesChannel = new Channel<T>();
        private readonly RequestReplyChannel<object, T> _requestChannel = new RequestReplyChannel<object, T>();

        ///<summary>
        ///</summary>
        ///<param name="timeoutInMs">For initial snapshot</param>
        public SnapshotChannel(int timeoutInMs)
        {
            _timeoutInMs = timeoutInMs;
        }

        ///<summary>
        /// Subscribes for an initial snapshot and then incremental update.
        ///</summary>
        ///<param name="fiber">the target executor to receive the message</param>
        ///<param name="receive"></param>
        public IDisposable PrimedSubscribe(IFiber fiber, Action<T> receive)
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

                receive(result);

                return _updatesChannel.Subscribe(fiber, receive);
            }
        }

        ///<summary>
        /// Publishes the incremental update.
        ///</summary>
        ///<param name="update"></param>
        public bool Publish(T update)
        {
            return _updatesChannel.Publish(update);
        }

        ///<summary>
        /// Ressponds to the request for an initial snapshot.
        ///</summary>
        ///<param name="fiber">the target executor to receive the message</param>
        ///<param name="reply">returns the snapshot update</param>
        public IDisposable ReplyToPrimingRequest(IFiber fiber, Func<T> reply)
        {
            return _requestChannel.Subscribe(fiber, request => request.SendReply(reply()));
        }
    }
}