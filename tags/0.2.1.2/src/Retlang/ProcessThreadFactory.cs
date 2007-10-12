namespace Retlang
{
    public interface IProcessThreadFactory
    {
        IProcessThread CreateProcessThread();
        IProcessThread CreateMessageBusThread();
    }

    public class ProcessThreadFactory: IProcessThreadFactory
    {
        private int _maxQueueDepth = -1;
        private int _maxEnqueueWaitTime = -1;
        private ICommandExecutor _executor;

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

        public ICommandExecutor Executor
        {
            get { return _executor; }
            set{ _executor = value; }
        }

        public IProcessThread CreateProcessThread()
        {
            CommandQueue queue = new CommandQueue();
            queue.MaxEnqueueWaitTime = _maxEnqueueWaitTime;
            queue.MaxDepth = _maxQueueDepth;
            if(_executor != null)
            {
                queue.Executor = _executor;
            }
            return new ProcessThread(queue);
        }

        public IProcessThread CreateMessageBusThread()
        {
            return CreateProcessThread();
        }
    }
}
