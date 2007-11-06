namespace Retlang
{
    public interface IProcessContextFactory : IThreadController
    {
        IProcessContext CreateAndStart();
        IProcessContext Create();
    }

    public class ProcessContextFactory : IProcessContextFactory
    {
        private MessageBus _bus;
        private ITransferEnvelopeFactory _envelopeFactory = new ObjectTransferEnvelopeFactory();
        private IProcessThreadFactory _threadFactory = new ProcessThreadFactory();
        private IProcessThread _busThread;

        public void Start()
        {
            if(_bus == null)
            {
                Init();
            }
            _busThread.Start();
        }

        public void Init()
        {
            _busThread = ThreadFactory.CreateMessageBusThread();
            _bus = new MessageBus(_busThread);
        }

        public IMessageBus MessageBus
        {
            get{ return _bus; }
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
            IProcessContext context = Create();
            context.Start();
            return context;
        }

        public IProcessContext Create()
        {
            return new ProcessContext(_bus, ThreadFactory.CreateProcessThread(), _envelopeFactory);
        }
    }
}