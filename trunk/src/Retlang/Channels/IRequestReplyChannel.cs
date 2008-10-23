namespace Retlang.Channels
{
    /// <summary>
    /// Typed channel for request/reply
    /// </summary>
    /// <typeparam name="R"></typeparam>
    /// <typeparam name="M"></typeparam>
    public interface IRequestReplyChannel<R, M> : IRequestPublisher<R, M>, IReplySubscriber<R, M>
    {
    }
}
