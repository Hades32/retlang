using System;

namespace Retlang
{
    public interface IProcessContextFactory : IThreadController, IObjectPublisher
    {
        IProcessContext CreateAndStart();
        IProcessContext Create();
        IProcessContext CreateAndStart(ICommandExecutor executor);
        IProcessContext Create(ICommandExecutor executor);

        IProcessContext CreateAndStart(string threadName);
        IProcessContext Create(string threadName);
        IProcessContext CreateAndStart(ICommandExecutor executor, string threadName);
        IProcessContext Create(ICommandExecutor executor, string threadName);


        IProcessBus CreatePooledAndStart(ICommandExecutor executor);
        IProcessBus CreatePooledAndStart();
        IProcessBus CreatePooled(ICommandExecutor executor);
        IProcessBus CreatePooled();

        IMessageBus MessageBus { get; }
    }

    public class ProcessContextFactory : IProcessContextFactory, IDisposable
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
            return StartThread(Create(executor));
        }

        public IProcessContext Create(ICommandExecutor executor)
        {
            return new ProcessContext(_bus, ThreadFactory.CreateProcessThread(executor), _envelopeFactory);
        }

        public IProcessContext CreateAndStart(string threadName)
        {
            return StartThread(Create(threadName));
        }

        public IProcessContext Create(string threadName)
        {
            return Create(new CommandExecutor(), threadName);
        }

        public IProcessContext CreateAndStart(ICommandExecutor executor, string threadName)
        {
            return StartThread(Create(executor, threadName));
        }

        private IProcessContext StartThread(IProcessContext context)
        {
            context.Start();
            return context;
        }

        public IProcessContext Create(ICommandExecutor executor, string threadName)
        {
            return new ProcessContext(_bus, ThreadFactory.CreateProcessThread(executor, threadName), _envelopeFactory);
        }

        public IProcessBus CreatePooled()
        {
            return CreatePooled(new CommandExecutor());
        }

        public IProcessBus CreatePooled(ICommandExecutor executor)
        {
            return new ProcessBus(_bus, new PoolQueue(_threadPool, executor), _envelopeFactory);
        }

        public IProcessBus CreatePooledAndStart()
        {
            return CreatePooledAndStart(new CommandExecutor());
        }

        public IProcessBus CreatePooledAndStart(ICommandExecutor executor)
        {
            IProcessBus bus = CreatePooled(executor);
            bus.Start();
            return bus;
        }

        public void Publish(object topic, object msg, object replyToTopic)
        {
            _bus.Publish(_envelopeFactory.Create(topic, msg, replyToTopic));    
        }

        public void Publish(object topic, object msg)
        {
            Publish(topic, msg, null);
        }

        public void Dispose()
        {
            Stop();
        }
    }
}