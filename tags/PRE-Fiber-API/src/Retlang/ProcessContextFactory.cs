using System;

namespace Retlang
{
    /// <summary>
    /// Creates process bus objects using the backing thread pool or dedicated threads. 
    /// <see cref="IProcessBus"/>
    /// </summary>
    public interface IProcessContextFactory : IThreadController, IObjectPublisher
    {
        /// <summary>
        /// Creates a new thread backed context. Starts the thread.
        /// </summary>
        /// <returns></returns>
        IProcessContext CreateAndStart();

        /// <summary>
        /// Creates a thread backed context. Does not start the thread.
        /// </summary>
        /// <returns></returns>
        IProcessContext Create();

        /// <summary>
        /// Creates and starts a thread backed context using the provided executor.
        /// </summary>
        /// <param name="executor"></param>
        /// <returns></returns>
        IProcessContext CreateAndStart(ICommandExecutor executor);

        /// <summary>
        /// Creates a thread backed context with the provided executor.
        /// </summary>
        /// <param name="executor"></param>
        /// <returns></returns>
        IProcessContext Create(ICommandExecutor executor);

        /// <summary>
        /// Creates and starts a named thread backed context.
        /// </summary>
        /// <param name="threadName"></param>
        /// <returns></returns>
        IProcessContext CreateAndStart(string threadName);

        /// <summary>
        /// Creates a context with a named thread.
        /// </summary>
        /// <param name="threadName"></param>
        /// <returns></returns>
        IProcessContext Create(string threadName);

        /// <summary>
        /// Creates and starts thread backed context with the provided thread name and executor
        /// </summary>
        /// <param name="executor"></param>
        /// <param name="threadName"></param>
        /// <returns></returns>
        IProcessContext CreateAndStart(ICommandExecutor executor, string threadName);

        /// <summary>
        /// Creates thread backed context with the provided thread name and executor.
        /// </summary>
        /// <param name="executor"></param>
        /// <param name="threadName"></param>
        /// <returns></returns>
        IProcessContext Create(ICommandExecutor executor, string threadName);

        /// <summary>
        /// Creates and start process bus using the provided executor and the default thread pool.
        /// </summary>
        /// <param name="executor"></param>
        /// <returns></returns>
        IProcessBus CreatePooledAndStart(ICommandExecutor executor);

        /// <summary>
        /// Create and start a process bus that using the thread pool.
        /// </summary>
        /// <returns></returns>
        IProcessBus CreatePooledAndStart();

        /// <summary>
        /// Create pool backed proces bus with the provided command executor.
        /// </summary>
        /// <param name="executor"></param>
        /// <returns></returns>
        IProcessBus CreatePooled(ICommandExecutor executor);

        /// <summary>
        /// Create a pool backed process bus.
        /// </summary>
        /// <returns></returns>
        IProcessBus CreatePooled();

        /// <summary>
        /// The underlying message bus used for message delivery.
        /// </summary>
        IMessageBus MessageBus { get; }
    }

    /// <summary>
    /// Default IProcessContextFactory implementation.
    /// </summary>
    public class ProcessContextFactory : IProcessContextFactory, IDisposable
    {
        private MessageBus _bus;
        private ITransferEnvelopeFactory _envelopeFactory = new ObjectTransferEnvelopeFactory();
        private IProcessThreadFactory _threadFactory = new ProcessThreadFactory();
        private IProcessThread _busThread;
        private IThreadPool _threadPool = new DefaultThreadPool();
        private ICommandExecutor _executor = new CommandExecutor();

        /// <summary>
        /// Command executor for message bus.
        /// </summary>
        public ICommandExecutor MessageBusCommandExecutor
        {
            get { return _executor; }
            set { _executor = value; }
        }

        /// <summary>
        /// Initializes message bus and starts delivery thread.
        /// </summary>
        public void Start()
        {
            if (_bus == null)
            {
                Init();
            }
            _busThread.Start();
        }

        /// <summary>
        /// Backing thread pool for pool backed process bus instances.
        /// </summary>
        public IThreadPool ThreadPool
        {
            get { return _threadPool; }
            set { _threadPool = value; }
        }

        /// <summary>
        /// Initializes the message bus.
        /// </summary>
        public void Init()
        {
            _busThread = ThreadFactory.CreateMessageBusThread(_executor);
            _bus = new MessageBus(_busThread);
        }

        /// <summary>
        /// Backing message bus.
        /// </summary>
        public IMessageBus MessageBus
        {
            get { return _bus; }
        }

        /// <summary>
        /// Stops message bus thread.
        /// </summary>
        public void Stop()
        {
            _busThread.Stop();
        }

        /// <summary>
        /// Wait for message bus thread.
        /// </summary>
        public void Join()
        {
            _busThread.Join();
        }

        /// <summary>
        /// Transfer envelope factory used by the process contexts.
        /// </summary>
        public ITransferEnvelopeFactory TransferEnvelopeFactory
        {
            get { return _envelopeFactory; }
            set { _envelopeFactory = value; }
        }

        /// <summary>
        /// Thread factory for message bus and process context instances.
        /// </summary>
        public IProcessThreadFactory ThreadFactory
        {
            get { return _threadFactory; }
            set { _threadFactory = value; }
        }

        /// <summary>
        /// <see cref="IProcessContextFactory.CreateAndStart()"/>
        /// </summary>
        /// <returns></returns>
        public IProcessContext CreateAndStart()
        {
            return CreateAndStart(new CommandExecutor());
        }

        /// <summary>
        /// <see cref="IProcessContextFactory.Create()"/>
        /// </summary>
        /// <returns></returns>
        public IProcessContext Create()
        {
            return Create(new CommandExecutor());
        }

        /// <summary>
        /// <see cref="IProcessContextFactory.CreateAndStart(ICommandExecutor)"/>
        /// </summary>
        /// <param name="executor"></param>
        /// <returns></returns>
        public IProcessContext CreateAndStart(ICommandExecutor executor)
        {
            return StartThread(Create(executor));
        }

        /// <summary>
        /// <see cref="IProcessContextFactory.Create(ICommandExecutor)"/>
        /// </summary>
        /// <param name="executor"></param>
        /// <returns></returns>
        public IProcessContext Create(ICommandExecutor executor)
        {
            return new ProcessContext(_bus, ThreadFactory.CreateProcessThread(executor), _envelopeFactory);
        }

        /// <summary>
        /// <see cref="IProcessContextFactory.CreateAndStart(string)"/>
        /// </summary>
        /// <param name="threadName"></param>
        /// <returns></returns>
        public IProcessContext CreateAndStart(string threadName)
        {
            return StartThread(Create(threadName));
        }

        /// <summary>
        /// <see cref="IProcessContextFactory.Create(string)"/>
        /// </summary>
        /// <param name="threadName"></param>
        /// <returns></returns>
        public IProcessContext Create(string threadName)
        {
            return Create(new CommandExecutor(), threadName);
        }

        /// <summary>
        /// <see cref="IProcessContextFactory.CreateAndStart(ICommandExecutor,string)"/>
        /// </summary>
        /// <param name="executor"></param>
        /// <param name="threadName"></param>
        /// <returns></returns>
        public IProcessContext CreateAndStart(ICommandExecutor executor, string threadName)
        {
            return StartThread(Create(executor, threadName));
        }

        private IProcessContext StartThread(IProcessContext context)
        {
            context.Start();
            return context;
        }

        /// <summary>
        /// <see cref="IProcessContextFactory.Create(ICommandExecutor,string)"/>
        /// </summary>
        /// <param name="executor"></param>
        /// <param name="threadName"></param>
        /// <returns></returns>
        public IProcessContext Create(ICommandExecutor executor, string threadName)
        {
            return new ProcessContext(_bus, ThreadFactory.CreateProcessThread(executor, threadName), _envelopeFactory);
        }

        /// <summary>
        /// <see cref="IProcessContextFactory.CreatePooled()"/>
        /// </summary>
        /// <returns></returns>
        public IProcessBus CreatePooled()
        {
            return CreatePooled(new CommandExecutor());
        }

        /// <summary>
        /// <see cref="IProcessContextFactory.CreatePooled(ICommandExecutor)"/>
        /// </summary>
        /// <param name="executor"></param>
        /// <returns></returns>
        public IProcessBus CreatePooled(ICommandExecutor executor)
        {
            return new ProcessBus(_bus, new PoolQueue(_threadPool, executor), _envelopeFactory);
        }

        /// <summary>
        /// <see cref="IProcessContextFactory.CreatePooledAndStart()"/>
        /// </summary>
        /// <returns></returns>
        public IProcessBus CreatePooledAndStart()
        {
            return CreatePooledAndStart(new CommandExecutor());
        }

        /// <summary>
        /// <see cref="IProcessContextFactory.CreatePooledAndStart(ICommandExecutor)"/>
        /// </summary>
        /// <param name="executor"></param>
        /// <returns></returns>
        public IProcessBus CreatePooledAndStart(ICommandExecutor executor)
        {
            IProcessBus bus = CreatePooled(executor);
            bus.Start();
            return bus;
        }

        /// <summary>
        /// Publishes message to the underlying message bus.
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="msg"></param>
        /// <param name="replyToTopic"></param>
        public void Publish(object topic, object msg, object replyToTopic)
        {
            _bus.Publish(_envelopeFactory.Create(topic, msg, replyToTopic));
        }

        /// <summary>
        /// Publishes message to the underlying message bus.
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="msg"></param>
        public void Publish(object topic, object msg)
        {
            Publish(topic, msg, null);
        }

        /// <summary>
        /// Stop message bus thread.
        /// </summary>
        public void Dispose()
        {
            Stop();
        }
    }
}