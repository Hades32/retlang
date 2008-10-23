namespace Retlang.Channels
{
    /// <summary>
    /// Channel publishing interface.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPublisher<T>
    {
        /// <summary>
        /// Publish a message to all subscribers. Returns true if any subscribers are registered.
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        bool Publish(T msg);
    }
}
