namespace Retlang.Channels
{
    /// <summary>
    /// A request object that can be used to send 1 or many responses to the initial request.
    /// </summary>
    /// <typeparam name="R"></typeparam>
    /// <typeparam name="M"></typeparam>
    public interface IRequest<R, M>
    {
        /// <summary>
        /// Request Message
        /// </summary>
        R Request { get; }

        /// <summary>
        /// Send one or more responses.
        /// </summary>
        /// <param name="replyMsg"></param>
        /// <returns></returns>
        bool SendReply(M replyMsg);
    }
}
