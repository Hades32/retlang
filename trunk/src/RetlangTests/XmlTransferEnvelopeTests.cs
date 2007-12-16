using NUnit.Framework;
using Retlang;

namespace RetlangTests
{
    [TestFixture]
    public class XmlTransferEnvelopeTests
    {
        [Test]
        public void RoundTrip()
        {
            ITransferEnvelope env = new XmlTransferEnvelopeFactory().Create(55, 1, 66);
            Assert.AreEqual(1.GetType(), env.MessageType);
            Assert.AreEqual(1, env.ResolveMessage());
            Assert.AreEqual(55, env.Header.Topic);
            Assert.AreEqual(66, env.Header.ReplyTo);
            Assert.IsTrue(env.CanCastTo<int>());
            Assert.IsTrue(env.CanCastTo<object>());
            Assert.IsFalse(env.CanCastTo<string>());
        }
    }
}