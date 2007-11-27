using System;
using System.Threading;
using NUnit.Framework;
using Retlang;
using Rhino.Mocks;

namespace RetlangTests
{
    [TestFixture]
    public class ProcessContextTests
    {
        [Test]
        public void ScheduleShutdown()
        {
            ProcessContextFactory factory = new ProcessContextFactory();
            factory.Start();
            IProcessContext context = factory.Create();
            context.Start();

            Command stopCommand = context.Stop;
            context.Schedule(stopCommand, 5);

            context.Join();

            factory.Stop();
            factory.Join();
        }

        [Test]
        public void ScheduleIntervalShutdown()
        {
            ProcessContextFactory factory = new ProcessContextFactory();
            factory.Start();
            IProcessContext context = factory.Create();
            context.Start();

            int count = 0;
            Command stopCommand = delegate
                                      {
                                          count++;
                                          if (count == 5)
                                          {
                                              context.Stop();
                                          }
                                      };
            context.ScheduleOnInterval(stopCommand, 1, 1);

            context.Join();

            factory.Stop();
            factory.Join();
            Assert.AreEqual(5, count);
        }

        [Test]
        public void ScheduleShutdownNoInterval()
        {
            ProcessContextFactory factory = new ProcessContextFactory();
            factory.Start();
            IProcessContext context = factory.Create();
            context.Start();

            Command stopCommand = delegate { context.Stop(); };
            context.Schedule(stopCommand, 0);

            context.Join();

            factory.Stop();
            factory.Join();
        }

        [Test]
        [Explicit]
        public void FirstEventDeliveryLoop()
        {
            for(int i = 0; i < 1000; i++)
            {
                FirstEventDelivery();
            }
        }

        [Test]
        public void FirstEventDelivery()
        {
            ProcessContextFactory factory = new ProcessContextFactory();
            factory.Start();
            IProcessContext context = factory.Create();
            IProcessContext pubContext = factory.CreateAndStart();
            AutoResetEvent reset = new AutoResetEvent(false);
            OnMessage<string> onMsg=delegate { reset.Set(); };
            context.Subscribe(new TopicEquals("topic"), onMsg);
            context.Start();

            pubContext.Publish("topic", "msg");

            Assert.IsTrue(reset.WaitOne(30000, false), "Should be reset by first message");

            pubContext.Stop();
            pubContext.Join();

            context.Stop();
            context.Join();

            factory.Stop();
            factory.Join();
        }

        [Test]
        public void EnqueuShutdown()
        {
            ProcessContextFactory factory = new ProcessContextFactory();
            factory.Start();
            IProcessContext context = factory.Create();
            context.Start();

            Command stopCommand = delegate { context.Stop(); };
            context.Enqueue(stopCommand);

            context.Join();

            factory.Stop();
            factory.Join();
        }


        [Test]
        public void PublishNullMsg()
        {
            ProcessContextFactory factory = ProcessFactoryFixture.CreateAndStart();
            IProcessContext process = factory.CreateAndStart();
            try
            {
                process.Publish("topic", null);
                Assert.Fail("should throw null reference exception");
            }
            catch (NullReferenceException exc)
            {
                Assert.IsNotNull(exc);
            }
            process.Stop();

            factory.Stop();
            process.Join();
            factory.Join();
        }

        public delegate bool OnSubscribe(ISubscriber subscriber);

        public delegate bool OnCommand(Command command);

        [Test]
        public void UnsubscribeAllOnStop()
        {
            MockRepository repo = new MockRepository();
            IMessageBus bus = repo.CreateMock<IMessageBus>();
            IProcessThread thread = repo.CreateMock<IProcessThread>();

            OnCommand executor = delegate(Command command)
                                     {
                                         command();
                                         return true;
                                     };
            thread.Enqueue(null);
            LastCall.IgnoreArguments().Callback(executor).Repeat.Any();

            bus.Subscribe(null);
            LastCall.IgnoreArguments();
            repo.Replay(bus);
            ProcessContext context = new ProcessContext(bus, thread, new ObjectTransferEnvelopeFactory());
            repo.BackToRecord(bus);

            OnMessage<int> onMessage = repo.CreateMock<OnMessage<int>>();

            bus.Unsubscribe(context);
            thread.Stop();
            repo.ReplayAll();

            context.Subscribe(new TopicEquals("topic"), onMessage);

            context.Stop();
            repo.VerifyAll();
        }

        [Test]
        public void QueueFullEvent()
        {
            MockRepository repo = new MockRepository();
            IMessageBus bus = repo.CreateMock<IMessageBus>();
            IProcessThread thread = repo.CreateMock<IProcessThread>();
            thread.Enqueue(null);
            QueueFullException exc = new QueueFullException(1);
            LastCall.IgnoreArguments().Throw(exc);

            OnQueueFull fullEvent = repo.CreateMock<OnQueueFull>();
            fullEvent(exc, new MessageHeader("topic", null), "data");

            repo.ReplayAll();

            ProcessContext context = new ProcessContext(bus, thread, new ObjectTransferEnvelopeFactory());
            context.QueueFullEvent += fullEvent;
            context.Subscribe<string>(new TopicEquals("topic"), delegate { });
            bool consumed = false;
            context.Receive(new ObjectTransferEnvelope("data", new MessageHeader("topic", null)), ref consumed);
            Assert.IsTrue(consumed);
            repo.VerifyAll();
        }
    }
}