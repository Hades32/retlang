using Retlang;

namespace RetlangTests
{
    public class ProcessFactoryFixture
    {
        public static ProcessContextFactory CreateAndStart()
        {
            ProcessThreadFactory threadFactory = new ProcessThreadFactory();
            ProcessContextFactory factory = new ProcessContextFactory();
            factory.ThreadFactory = threadFactory;
            factory.TransferEnvelopeFactory = new ObjectTransferEnvelopeFactory();
            threadFactory.MaxQueueDepth = 10000;
            threadFactory.MaxEnqueueWaitTime = 10000;
            factory.Start();
            return factory;
        }
    }
}