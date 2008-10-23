using System;
using Retlang.Core;

namespace Retlang.Channels
{
    /// <summary>
    /// Methods for working with a replyChannel
    /// </summary>
    /// <typeparam name="R"></typeparam>
    /// <typeparam name="M"></typeparam>
    public interface IReplySubscriber<R, M>
    {
        /// <summary>
        /// Subscribe to a request on the channel.
        /// </summary>
        /// <param name="responder"></param>
        /// <param name="onRequest"></param>
        /// <returns></returns>
        IUnsubscriber Subscribe(IDisposingExecutor responder, Action<IRequest<R, M>> onRequest);
    }
}
