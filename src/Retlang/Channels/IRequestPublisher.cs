namespace Retlang.Channels
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="R"></typeparam>
    /// <typeparam name="M"></typeparam>
    public interface IRequestPublisher<R, M>
    {
        /// <summary>
        /// Send request on the channel.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        IReply<M> SendRequest(R request);
    }
}
