using System;
using System.Collections.Generic;
using Retlang;

namespace RetlangTests
{
    public class ProcessFactoryFixture
    {
        public static ProcessContextFactory CreateAndStart()
        {
            ProcessContextFactory factory = new ProcessContextFactory();
            factory.TransferEnvelopeFactory = new ObjectTransferEnvelopeFactory();
            factory.Start();
            return factory;
        }
    }
}
