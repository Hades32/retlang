using System.Threading;

namespace Retlang
{
    /// <summary>
    /// A thread pool for executing asynchronous events.
    /// </summary>
    public interface IThreadPool
    {
        /// <summary>
        /// Queue event for execution.
        /// </summary>
        /// <param name="callback"></param>
        void Queue(WaitCallback callback);
    }

    /// <summary>
    /// Default implementation that uses the .NET thread pool.
    /// </summary>
    public class DefaultThreadPool : IThreadPool
    {
        /// <summary>
        /// Queue event.
        /// </summary>
        /// <param name="callback"></param>
        public void Queue(WaitCallback callback)
        {
            if (!ThreadPool.QueueUserWorkItem(callback))
            {
                throw new QueueFullException("Unable to add item to pool: " + callback.Target);
            }
        }
    }
}