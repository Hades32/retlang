using System;

namespace Retlang.Channels
{
    /// <summary>
    /// A channel provides a conduit for messages. It provides methods for publishing and subscribing to messages. 
    /// The class is thread safe.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IChannel<T> : ISubscriber<T>, IPublisher<T>
    {
        /// <summary>
        /// Subscribes to actions on producer threads. Subscriber could be called from multiple threads.
        /// </summary>
        /// <param name="subscriber"></param>
        /// <returns></returns>
        IDisposable SubscribeOnProducerThreads(IProducerThreadSubscriber<T> subscriber);
    }
}
