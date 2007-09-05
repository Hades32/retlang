using System;
using System.Collections.Generic;
using System.Text;
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
            MessageBus bus = new MessageBus();
            object topic = new object();
            bus.Publish(new ObjectTransferEnvelope(1, new MessageHeader(topic, null)));
        }

        [Test]
        public void EmptyPublishWithHandler()
        {
            ITransferEnvelope unHandledMessage = null;
            SynchronousCommandQueue queue = new SynchronousCommandQueue();
            MessageBus bus = new MessageBus();
            bus.Start();
            bus.UnhandledMessageEvent += delegate(ITransferEnvelope env){
                unHandledMessage = env;
            };
            object topic = new object();
            bus.Publish(new ObjectTransferEnvelope(1, new MessageHeader(topic, null)));
            Assert.IsNotNull(unHandledMessage);
            bus.Stop();
            bus.Join();
        }

        [Test]
        public void PubSub()
        {
            SynchronousCommandQueue queue = new SynchronousCommandQueue();
            MessageBus bus = new MessageBus();
            bus.Start();
            string count = "";
            OnMessage<string> onInt = delegate(IMessageHeader header, string num){
                count += num.ToString();
            };
            object topic = new object();
            ISubscriber subscriber = new TopicSubscriber<string>(new TopicEquals(topic), onInt, queue);
            bus.Subscribe(subscriber);
            bus.Publish(CreateMessage(topic, "1"));
            Assert.AreEqual("1", count);
            bus.Publish(CreateMessage(topic, "2"));
            Assert.AreEqual("12", count);
            bus.Unsubscribe(subscriber);
            bus.Publish(CreateMessage(topic, "2"));
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
