namespace Retlang
{
    public interface IProcessContextFactory : IThreadController
    {
        IProcessContext CreateAndStart();
        IProcessContext Create();
        IProcessContext CreateAndStart(ICommandExecutor executor);
        IProcessContext Create(ICommandExecutor executor);

        IProcessContext CreatePooledAndStart(ICommandExecutor executor);
        IProcessContext CreatePooled(ICommandExecutor executor);
    }

    public class ProcessContextFactory : IProcessContextFactory
    {
        private MessageBus _bus;
        private ITransferEnvelopeFactory _envelopeFactory = new ObjectTransferEnvelopeFactory();
        private IProcessThreadFactory _threadFactory = new ProcessThreadFactory();
        private IProcessThread _busThread;
        private IThreadPool _threadPool = new DefaultThreadPool();
        private ICommandExecutor _executor = new CommandExecutor();

        public ICommandExecutor MessageBusCommandExecutor
        {
            get { return _executor; }
            set { _executor = value; }
        }

        public void Start()
        {
            if (_bus == null)
            {
                Init();
            }
            _busThread.Start();
        }

        public IThreadPool ThreadPool
        {
            get { return _threadPool; }
            set { _threadPool = value; }
        }

        public void Init()
        {
            _busThread = ThreadFactory.CreateMessageBusThread(_executor);
            _bus = new MessageBus(_busThread);
        }

        public IMessageBus MessageBus
        {
            get { return _bus; }
        }

        public void Stop()
        {
            _busThread.Stop();
        }

        public void Join()
        {
            _busThread.Join();
        }

        public ITransferEnvelopeFactory TransferEnvelopeFactory
        {
            get { return _envelopeFactory; }
            set { _envelopeFactory = value; }
        }

        public IProcessThreadFactory ThreadFactory
        {
            get { return _threadFactory; }
            set { _threadFactory = value; }
        }

        public IProcessContext CreateAndStart()
        {
            return CreateAndStart(new CommandExecutor());
        }

        public IProcessContext Create()
        {
            return Create(new CommandExecutor());
        }

        public IProcessContext CreateAndStart(ICommandExecutor executor)
        {
            IProcessContext context = Create(executor);
            context.Start();
            return context;
        }

        public IProcessContext Create(ICommandExecutor executor)
        {
            return new ProcessContext(_bus, ThreadFactory.CreateProcessThread(executor), _envelopeFactory);
        }

        public IProcessContext CreatePooled(ICommandExecutor executor)
        {
            return new ProcessContext(_bus, new PoolQueue(_threadPool, executor), _envelopeFactory);
        }

        public IProcessContext CreatePooledAndStart(ICommandExecutor executor)
        {
            IProcessContext context = CreatePooled(executor);
            context.Start();
            return context;
        }
    }
}