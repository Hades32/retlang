using System.Threading;
using NUnit.Framework;
using Retlang;
using Rhino.Mocks;

namespace RetlangTests
{
    [TestFixture]
    public class LastSubscriberTests
    {
        [Test]
        public void TestReceivedFlushed()
        {
            MockRepository mockRepository = new MockRepository();

            string message = "Foo";
            bool receivedCallback = false;
            ManualResetEvent manualResetEvent = new ManualResetEvent(false);

            OnMessage<string> callback = delegate(IMessageHeader header, string received)
                                             {
                                                 Assert.AreEqual(message, received);
                                                 receivedCallback = true;
                                                 manualResetEvent.Set();
                                             };

            ICommandTimer timer = mockRepository.CreateMock<ICommandTimer>();
            IMessageHeader messageHeader = mockRepository.CreateMock<IMessageHeader>();

            LastSubscriber<string> subscriber = new LastSubscriber<string>(callback, timer, 0);

            Expect.Call(timer.Schedule(subscriber.Flush, 0)).Return(null);

            mockRepository.ReplayAll();

            subscriber.ReceiveMessage(messageHeader, message);
            subscriber.Flush();

            manualResetEvent.WaitOne(1000, false);

            Assert.IsTrue(receivedCallback);

            mockRepository.VerifyAll();
        }

        [Test]
        public void TestReceiveJustOne()
        {
            MockRepository mockRepository = new MockRepository();

            string firstMessage = "Foo";
            string secondMessage = "Bar";
            string lastMessage = "Baz";
            bool receivedCallback = false;
            int timesCalled = 0;
            ManualResetEvent manualResetEvent = new ManualResetEvent(false);

            OnMessage<string> callback = delegate(IMessageHeader header, string received)
                                             {
                                                 Assert.AreEqual(lastMessage, received);
                                                 receivedCallback = true;
                                                 timesCalled++;
                                                 manualResetEvent.Set();
                                             };

            ICommandTimer timer = mockRepository.CreateMock<ICommandTimer>();
            IMessageHeader messageHeader = mockRepository.CreateMock<IMessageHeader>();

            LastSubscriber<string> subscriber = new LastSubscriber<string>(callback, timer, 0);

            Expect.Call(timer.Schedule(subscriber.Flush, 0)).Return(null);

            mockRepository.ReplayAll();

            subscriber.ReceiveMessage(messageHeader, firstMessage);
            subscriber.ReceiveMessage(messageHeader, secondMessage);
            subscriber.ReceiveMessage(messageHeader, lastMessage);
            subscriber.Flush();

            manualResetEvent.WaitOne(1000, false);

            Assert.IsTrue(receivedCallback);
            Assert.AreEqual(1, timesCalled);

            mockRepository.VerifyAll();
        }
    }
}