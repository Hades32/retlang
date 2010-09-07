using System;
using Retlang.Core;

namespace Retlang.Channels
{
    /// <summary>
    /// Creates a queue that will deliver a message to a single consumer. Load balancing can be achieved by creating 
    /// multiple subscribers to the queue.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IQueueChannel<T>
    {
        /// <summary>
        /// Subscribe to the context.
        /// </summary>
        /// <param name="executionContext"></param>
        /// <param name="onMessage"></param>
        /// <returns></returns>
        IDisposable Subscribe(IExecutionContext executionContext, Action<T> onMessage);

        /// <summary>
        /// Pushes a message into the queue. Message will be processed by first available consumer.
        /// </summary>
        /// <param name="message"></param>
        void Publish(T message);
    }
}
