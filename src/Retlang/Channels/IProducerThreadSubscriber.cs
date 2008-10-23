namespace Retlang.Channels
{
    /// <summary>
    /// Event Subscriber that receives events on producer thread.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IProducerThreadSubscriber<T>
    {
        /// <summary>
        /// Method called from producer threads
        /// </summary>
        /// <param name="msg"></param>
        void ReceiveOnProducerThread(T msg);
    }
}
