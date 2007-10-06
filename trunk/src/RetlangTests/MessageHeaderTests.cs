using Retlang;
using NUnit.Framework;

namespace RetlangTests
{
    [TestFixture]
    public class MessageHeaderTests
    {

        [Test]
        public void Equality()
        {
            MessageHeader header = new MessageHeader("topic", "replyTo");
            Assert.AreEqual(header, new MessageHeader("topic", "replyTo"));
            Assert.AreNotEqual(header, new MessageHeader(new object(), "replyTo"));
            Assert.AreNotEqual(header, new MessageHeader("topic", null));
        }
    }
}
