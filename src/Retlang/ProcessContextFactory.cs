using System;
using System.Collections.Generic;
using System.Text;

namespace Retlang
{
    public interface IProcessContextFactory: IThreadController
    {
        IProcessContext CreateAndStart();
        IProcessContext Create();
    }
    public class ProcessContextFactory: IProcessContextFactory
    {
        private int _maxQueueDepth = -1;
        private readonly MessageBus _bus = new MessageBus();
        private ITransferEnvelopeFactory _envelopeFactory = new BinaryTransferEnvelopeFactory();

        public void Start()
        {
            _bus.Start();
        }

        public void Stop()
        {
            _bus.Stop();
        }

        public void Join()
        {
            _bus.Join();
        }

        public int MaxQueueDepth
        {
            get { return _maxQueueDepth; }
            set { _maxQueueDepth = value; }
        }

        public IMessageBus MessageBus
        {
            get { return _bus; }
        }

        public ITransferEnvelopeFactory TransferEnvelopeFactory
        {
            get { return _envelopeFactory; }
            set { _envelopeFactory = value; }
        }

        public IProcessContext CreateAndStart()
        {
            IProcessContext context = Create();
            context.Start();
            return context;
        }

        public IProcessContext Create()
        {
            CommandQueue queue = new CommandQueue();
            queue.MaxDepth = _maxQueueDepth;
            return new ProcessContext(_bus, new ProcessThread(queue), _envelopeFactory);
        }
    }
}
