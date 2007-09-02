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
            bus.Publish(topic, 1);
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
            ISubscriber subscriber = new TopicSubscriber<string>(new TopicMatcher(topic), onInt, queue);
            bus.Subscribe(subscriber);
            bus.Publish(topic, "1");
            Assert.AreEqual("1", count);
            bus.Publish(topic, "2");
            Assert.AreEqual("12", count);
            bus.Unsubscribe(subscriber);
            bus.Publish(topic, "2");
            Assert.AreEqual("12", count);
            bus.Stop();
            bus.Join();
        }

    }
}
