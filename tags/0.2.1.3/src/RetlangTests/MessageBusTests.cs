using System.Threading;
using NUnit.Framework;
using Retlang;

namespace RetlangTests
{
    [TestFixture]
    public class MessageBusTests
    {
        [Test]
        public void EmptyPublish()
        {
            SynchronousCommandQueue queue = new SynchronousCommandQueue();
            MessageBus bus = new MessageBus(new ProcessThread(queue));
            object topic = new object();
            bus.Publish(new ObjectTransferEnvelope(1, new MessageHeader(topic, null)));
        }

        [Test]
        public void PubSubWithSynchronousQueue()
        {
            SynchronousCommandQueue queue = new SynchronousCommandQueue();
            MessageBus bus = new MessageBus(new ProcessThread(queue));
            object topic = new object();
            bool received = false;
            OnMessage<int> receivedMessage = delegate
                                                 {
                                                     received = true;
                                                 };
            bus.Subscribe(new TopicSubscriber<int>(new TopicEquals(topic), receivedMessage));
            bus.Publish(new ObjectTransferEnvelope(1, new MessageHeader(topic, null)));
            Assert.IsTrue(received);
        }

        [Test]
        public void EmptyPublishWithHandler()
        {
            ITransferEnvelope unHandledMessage = null;
            CommandQueue queue = new CommandQueue();
            ProcessThread thread = new ProcessThread(queue);
            MessageBus bus = new MessageBus(thread);
            thread.Start();
            AutoResetEvent reset = new AutoResetEvent(false);
            bus.UnhandledMessageEvent += delegate(ITransferEnvelope env)
                                             {
                                                 unHandledMessage = env;
                                                 reset.Set();
                                             };
            object topic = new object();
            bus.Publish(new ObjectTransferEnvelope(1, new MessageHeader(topic, null)));

            Assert.IsTrue(reset.WaitOne(30000, false));

            Assert.IsNotNull(unHandledMessage);
            thread.Stop();
            thread.Join();
        }

        [Test]
        public void PubSub()
        {
            CommandQueue comQueue = new CommandQueue();
            ProcessThread thread = new ProcessThread(comQueue);
            MessageBus bus = new MessageBus(thread);
            thread.Start();
            string count = "";
            SynchronousCommandQueue queue = new SynchronousCommandQueue();
            AutoResetEvent reset = new AutoResetEvent(false);
            int messageCount = 0;
            OnMessage<string> onInt = delegate(IMessageHeader header, string num)
                                          {
                                              count += num.ToString();
                                              messageCount++;
                                              if (messageCount == 2)
                                              {
                                                  reset.Set();
                                              }
                                          };
            object topic = new object();
            ISubscriber subscriber = new TopicSubscriber<string>(new TopicEquals(topic), onInt);
            bus.Subscribe(subscriber);
            bus.Publish(CreateMessage(topic, "1"));
            bus.Publish(CreateMessage(topic, "2"));
            Assert.IsTrue(reset.WaitOne(1000, false));
            Assert.AreEqual("12", count);
            thread.Stop();
            thread.Join();
        }

        private ITransferEnvelope CreateMessage(object topic, object msg)
        {
            return new ObjectTransferEnvelope(msg, new MessageHeader(topic, null));
        }
    }
}