namespace Retlang.Channels
{
    /// <summary>
    /// Message filter delegate
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="msg"></param>
    /// <returns></returns>
    public delegate bool Filter<T>(T msg);

    /// <summary>
    /// Callback method and parameters for a channel subscription
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISubscribable<T> : IProducerThreadSubscriber<T>
    {
        /// <summary>
        /// Filter called from producer threads. Should be thread safe as it may be called from
        /// multiple threads.
        /// </summary>
        Filter<T> FilterOnProducerThread { get; set; }
    }
}