using System.Collections.Generic;
using NUnit.Framework;
using Retlang;
using Rhino.Mocks;

namespace RetlangTests
{
    [TestFixture]
    public class KeyedBatchSubscriberTests
    {
        [Test]
        public void Batch()
        {
            MockRepository repo = new MockRepository();
            IProcessContext context = repo.CreateMock<IProcessContext>();
            IMessageHeader header = repo.CreateMock<IMessageHeader>();

            ResolveKey<string, int> resolver = delegate(IMessageHeader head, int val) { return val.ToString(); };

            KeyedBatchSubscriber<string, int> batch = new KeyedBatchSubscriber<string, int>(resolver,
                                                                                            CheckValues, context, 0);

            Expect.Call(context.Schedule(batch.Flush, 0)).Return(null);

            repo.ReplayAll();

            batch.ReceiveMessage(header, 1);
            batch.ReceiveMessage(header, 2);
            batch.ReceiveMessage(header, 1);
            batch.Flush();
        }

        private void CheckValues(IDictionary<string, IMessageEnvelope<int>> messages)
        {
            Assert.AreEqual(2, messages.Values.Count);
            Assert.AreEqual(1, messages["1"].Message);
            Assert.AreEqual(2, messages["2"].Message);
        }
    }
}