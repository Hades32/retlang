using Retlang.Core;

namespace Retlang.Fibers
{
    /// <summary>
    /// Default implementation for IThreadFiberFactory
    /// </summary>
    public class ThreadFiberFactory : IThreadFiberFactory
    {
        private int _maxQueueDepth = -1;
        private int _maxEnqueueWaitTime = -1;

        /// <summary>
        /// Maximum depth for queue.
        /// </summary>
        public int MaxQueueDepth
        {
            get { return _maxQueueDepth; }
            set { _maxQueueDepth = value; }
        }

        /// <summary>
        /// Max time to wait for queue to clear.
        /// </summary>
        public int MaxEnqueueWaitTime
        {
            get { return _maxEnqueueWaitTime; }
            set { _maxEnqueueWaitTime = value; }
        }

        /// <summary>
        /// <see cref="IThreadFiberFactory.CreateThreadFiber(IBatchExecutor)"/>
        /// </summary>
        /// <param name="executor"></param>
        /// <returns></returns>
        public IThreadFiber CreateThreadFiber(IBatchExecutor executor)
        {
            CommandQueue queue = CreateQueue(executor);
            return new ThreadFiber(queue);
        }

        /// <summary>
        /// <see cref="IThreadFiberFactory.CreateThreadFiber(IBatchExecutor,string)"/>
        /// </summary>
        /// <param name="executor"></param>
        /// <param name="threadName"></param>
        /// <returns></returns>
        public IThreadFiber CreateThreadFiber(IBatchExecutor executor, string threadName)
        {
            CommandQueue queue = CreateQueue(executor);
            return new ThreadFiber(queue, threadName);
        }

        private CommandQueue CreateQueue(IBatchExecutor executor)
        {
            CommandQueue queue = new CommandQueue();
            queue.MaxEnqueueWaitTime = _maxEnqueueWaitTime;
            queue.MaxDepth = _maxQueueDepth;
            if (executor != null)
            {
                queue.Executor = executor;
            }
            return queue;
        }
    }
}