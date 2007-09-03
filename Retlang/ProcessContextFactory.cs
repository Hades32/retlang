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
            return new ProcessContext(_bus, new ProcessThread(new CommandQueue()), _envelopeFactory);
        }
    }
}
