using Retlang.Core;

namespace Retlang.Channels
{
    /// <summary>
    /// Action subscriber that receives actions on producer thread.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IProducerThreadSubscriber<T>
    {
        /// <summary>
        /// Method called from producer threads
        /// </summary>
        /// <param name="msg"></param>
        void ReceiveOnProducerThread(T msg);

        ///<summary>
        /// Allows for the registration and deregistration of subscriptions
        ///</summary>
        ISubscriptionRegistry Subscriptions { get; }
    }
}
