namespace Retlang
{
    /// <summary>
    /// Factory for creating backing threads.
    /// </summary>
    public interface IProcessThreadFactory
    {
        /// <summary>
        /// Create ProcessThread with ICommandExecutor
        /// </summary>
        /// <param name="executor"></param>
        /// <returns></returns>
        IProcessThread CreateProcessThread(ICommandExecutor executor);
        /// <summary>
        /// Create thread for message bus
        /// </summary>
        /// <param name="executor"></param>
        /// <returns></returns>
        IProcessThread CreateMessageBusThread(ICommandExecutor executor);
        /// <summary>
        /// Create named process thread.
        /// </summary>
        /// <param name="executor"></param>
        /// <param name="threadName"></param>
        /// <returns></returns>
        IProcessThread CreateProcessThread(ICommandExecutor executor, string threadName);
        /// <summary>
        /// Create named message bus thread.
        /// </summary>
        /// <param name="executor"></param>
        /// <param name="threadName"></param>
        /// <returns></returns>
        IProcessThread CreateMessageBusThread(ICommandExecutor executor, string threadName);
    }

    /// <summary>
    /// Default implementation for IProcessThreadFactory
    /// </summary>
    public class ProcessThreadFactory : IProcessThreadFactory
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
        /// <see cref="IProcessThreadFactory.CreateProcessThread(ICommandExecutor)"/>
        /// </summary>
        /// <param name="executor"></param>
        /// <returns></returns>
        public IProcessThread CreateProcessThread(ICommandExecutor executor)
        {
            CommandQueue queue = CreateQueue(executor);
            return new ProcessThread(queue);
        }

        private CommandQueue CreateQueue(ICommandExecutor executor)
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
        /// <summary>
        /// <see cref="IProcessThreadFactory.CreateMessageBusThread(ICommandExecutor)"/>
        /// </summary>
        /// <param name="executor"></param>
        /// <returns></returns>
        public IProcessThread CreateMessageBusThread(ICommandExecutor executor)
        {
            return CreateProcessThread(executor);
        }
        /// <summary>
        /// <see cref="IProcessThreadFactory.CreateProcessThread(ICommandExecutor,string)"/>
        /// </summary>
        /// <param name="executor"></param>
        /// <param name="threadName"></param>
        /// <returns></returns>
        public IProcessThread CreateProcessThread(ICommandExecutor executor, string threadName)
        {
            CommandQueue queue = CreateQueue(executor);
            return new ProcessThread(queue, threadName);
        }
        /// <summary>
        /// <see cref="IProcessThreadFactory.CreateMessageBusThread(ICommandExecutor,string)"/>
        /// </summary>
        /// <param name="executor"></param>
        /// <param name="threadName"></param>
        /// <returns></returns>
        public IProcessThread CreateMessageBusThread(ICommandExecutor executor, string threadName)
        {
            CommandQueue queue = CreateQueue(executor);
            return new ProcessThread(queue, threadName);
        }
    }
}