using System;
using System.Collections.Generic;
using NUnit.Framework;
using Retlang;

namespace RetlangTests
{
    [TestFixture]
    public class ProcessThreadTests
    {

        [Test]
        public void RunThread()
        {
            ProcessThread thread = new ProcessThread(new CommandQueue());
            thread.Start();
            thread.Stop();
            thread.Join();
        }

        [Test]
        public void AsyncStop()
        {
            ProcessThread thread = new ProcessThread(new CommandQueue());
            thread.Start();
            thread.Enqueue(thread.Stop);
            thread.Join();
        }

        [Test]
        public void PubSub()
        {
            CommandQueue queue = new CommandQueue();
            ProcessThread thread = new ProcessThread(queue);
            thread.Start();
            ProcessContext proc = new ProcessContext(new MessageBus(), thread);
            object topic = new object();
            OnMessage<string> onMsg = delegate
            {
                thread.Stop();
            };
            proc.Subscribe(new TopicMatcher(topic), onMsg);
            proc.Publish(topic, "stuff");

            thread.Join();
        }

        [Test]
        public void PubSub2Thread()
        {
            ProcessContextFactory fact = new ProcessContextFactory();

            IProcessContext context = fact.Create();
            context.Start();
            IProcessContext context2 = fact.Create();
            context2.Start();

            object topic = new object();
            object stopTopic = new object();
            OnMessage<string> stopAll = delegate
            {
                context.Stop();
                context2.Stop();
            };
            OnMessage<string> onMsg = delegate
            {
                context2.Publish(stopTopic, "morestuff");
            };

            context.Subscribe(new TopicMatcher(topic), onMsg);
            context2.Subscribe(new TopicMatcher(stopTopic), stopAll);

            context.Publish(topic, "stuff");

            context.Join();
            context2.Join();
        }

        [Test]
        public void RequestReplyWithCommands()
        {
            ProcessContextFactory fact = new ProcessContextFactory();
            fact.Start();

            IProcessContext context = fact.Create();
            context.Start();
            IProcessContext context2 = fact.Create();
            context2.Start();

            object topic = new object();
            OnMessage<OnMessage<IMessageHeader>> onMsg = delegate(IMessageHeader header, OnMessage<IMessageHeader> msg)
            {
                msg(header, header);
            };

            context2.Subscribe(new TopicMatcher(topic), onMsg);

            OnMessage<IMessageHeader> replyCommand = delegate(IMessageHeader header, IMessageHeader headerCopy)
            {
                context2.Publish(header.ReplyTo, "reply to: stuff");
            };
            IRequestReply<string> req = context.SendRequest<string>(topic, replyCommand);

            Assert.AreEqual("reply to: stuff", req.Receive(1000).Message);

            context.Stop();
            context.Join();
            context2.Stop();
            context2.Join();
            fact.Stop();
            fact.Join();
        }

        [Test]
        public void RequestReply()
        {
            ProcessContextFactory fact = new ProcessContextFactory();
            fact.Start();

            IProcessContext context = fact.Create();
            context.Start();
            IProcessContext context2 = fact.Create();
            context2.Start();

            object topic = new object();
            OnMessage<string> onMsg = delegate(IMessageHeader header, string msg)
            {
                context2.Publish(header.ReplyTo, "reply to: " + msg);
            };

            context2.Subscribe(new TopicMatcher(topic), onMsg);

            IRequestReply<string> req = context.SendRequest<string>(topic, "stuff");

            Assert.AreEqual("reply to: stuff", req.Receive(1000).Message);

            context.Stop();
            context.Join();
            context2.Stop();
            context2.Join();
            fact.Stop();
            fact.Join();
        }
    }
}
