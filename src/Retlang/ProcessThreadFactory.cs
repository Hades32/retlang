namespace Retlang
{
    public interface IProcessThreadFactory
    {
        IProcessThread CreateProcessThread(ICommandExecutor executor);
        IProcessThread CreateMessageBusThread(ICommandExecutor executor);
    }

    public class ProcessThreadFactory: IProcessThreadFactory
    {
        private int _maxQueueDepth = -1;
        private int _maxEnqueueWaitTime = -1;

        public int MaxQueueDepth
        {
            get { return _maxQueueDepth; }
            set { _maxQueueDepth = value; }
        }

        public int MaxEnqueueWaitTime
        {
            get { return _maxEnqueueWaitTime; }
            set { _maxEnqueueWaitTime = value; }
        }

        public IProcessThread CreateProcessThread(ICommandExecutor executor)
        {
            CommandQueue queue = new CommandQueue();
            queue.MaxEnqueueWaitTime = _maxEnqueueWaitTime;
            queue.MaxDepth = _maxQueueDepth;
            if(executor != null)
            {
                queue.Executor = executor;
            }
            return new ProcessThread(queue);
        }

        public IProcessThread CreateMessageBusThread(ICommandExecutor executor)
        {
            return CreateProcessThread(executor);
        }
    }
}
