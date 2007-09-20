using NUnit.Framework;
using Retlang;

namespace RetlangTests
{
    [TestFixture]
    public class BinaryTransferEnvelopeTests
    {
        [Test]
        public void RoundTrip()
        {
            ITransferEnvelope env = new BinaryTransferEnvelopeFactory().Create(55, 1, 66);
            Assert.AreEqual(1.GetType(), env.MessageType);
            Assert.AreEqual(1, env.ResolveMessage());
            Assert.AreEqual(55, env.Header.Topic);
            Assert.AreEqual(66, env.Header.ReplyTo);
        }
    }
}