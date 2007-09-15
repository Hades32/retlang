using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Retlang;
using System.Threading;

namespace RetlangTests
{
    [TestFixture]
    public class MessageBusTests
    {
        [Test]
        public void EmptyPublish()
        {
            SynchronousCommandQueue queue = new SynchronousCommandQueue();
            CommandQueue comQueue = new CommandQueue();
            MessageBus bus = new MessageBus(comQueue, new ProcessThread(comQueue));
            object topic = new object();
            bus.Publish(new ObjectTransferEnvelope(1, new MessageHeader(topic, null)));
        }

        [Test]
        public void EmptyPublishWithHandler()
        {
            ITransferEnvelope unHandledMessage = null;
            CommandQueue queue = new CommandQueue();
            MessageBus bus = new MessageBus(queue, new ProcessThread(queue));
            bus.Start();
            AutoResetEvent reset = new AutoResetEvent(false);
            bus.UnhandledMessageEvent += delegate(ITransferEnvelope env){
                unHandledMessage = env;
                reset.Set();
            };
            object topic = new object();
            bus.Publish(new ObjectTransferEnvelope(1, new MessageHeader(topic, null)));

            Assert.IsTrue(reset.WaitOne(30000, false));

            Assert.IsNotNull(unHandledMessage);
            bus.Stop();
            bus.Join();
        }

        [Test]
        public void PubSub()
        {
            CommandQueue comQueue = new CommandQueue();
            MessageBus bus = new MessageBus(comQueue, new ProcessThread(comQueue));
            bus.Start();
            string count = "";
            SynchronousCommandQueue queue = new SynchronousCommandQueue();
            AutoResetEvent reset = new AutoResetEvent(false);
            int messageCount = 0;
            OnMessage<string> onInt = delegate(IMessageHeader header, string num){
                count += num.ToString();
                messageCount++;
                if (messageCount == 2)
                {
                    reset.Set();
                }
            };
            object topic = new object();
            ISubscriber subscriber = new TopicSubscriber<string>(new TopicEquals(topic), onInt, queue);
            bus.Subscribe(subscriber);
            bus.Publish(CreateMessage(topic, "1"));
            bus.Publish(CreateMessage(topic, "2"));
            Assert.IsTrue(reset.WaitOne(1000, false));
            Assert.AreEqual("12", count);
            bus.Stop();
            bus.Join();
        }

        private ITransferEnvelope CreateMessage(object topic, object msg)
        {
            return new ObjectTransferEnvelope(msg, new MessageHeader(topic, null));
        }

    }
}
