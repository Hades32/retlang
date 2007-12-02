using System;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;
using Retlang;

namespace RetlangTests
{
    [TestFixture]
    public class PerfTests : ICommandExecutor
    {
        private IProcessContext CreateContext(ProcessContextFactory factory)
        {
            return factory.CreateAndStart(this);
        }

        [Test]
        [Explicit]
        public void PubSub()
        {
            ProcessContextFactory factory = ProcessFactoryFixture.CreateAndStart();
            MessageBus bus = factory.MessageBus as MessageBus;
            bus.AsyncPublish = false;

            IProcessContext pubContext = CreateContext(factory);
            IProcessContext receiveContext = CreateContext(factory);
            int totalMessages = 10000000;

            OnMessage<int> received = delegate(IMessageHeader header, int count)
                                          {
                                              if (count == totalMessages)
                                              {
                                                  receiveContext.Stop();
                                              }
                                          };
            object topic = new object();
            TopicEquals selectall = new TopicEquals(topic);
            receiveContext.Subscribe<int>(selectall, received);

            Stopwatch watch = new Stopwatch();
            watch.Start();
            for (int i = 1; i <= totalMessages; i++)
            {
                pubContext.Publish(topic, i);
            }
            Console.WriteLine("Done publishing.");
            receiveContext.Join();
            pubContext.Stop();
            pubContext.Join();
            factory.Stop();
            factory.Join();

            Console.WriteLine("Time: " + watch.ElapsedMilliseconds + " count: " + totalMessages);
            Console.WriteLine("Avg Per Second: " + (totalMessages/watch.Elapsed.TotalSeconds));
        }

        [Test]
        [Explicit]
        public void PubSubWithPool()
        {
            DefaultThreadPool pool = new DefaultThreadPool();
            PoolQueue busQueue = new PoolQueue(pool, this);
            busQueue.Start();
            MessageBus bus = new MessageBus(busQueue);
            bus.AsyncPublish = false;

            ObjectTransferEnvelopeFactory transfer = new ObjectTransferEnvelopeFactory();
            IProcessBus pubContext = new ProcessBus(bus, new PoolQueue(pool, new CommandExecutor()), transfer);
            pubContext.Start();
            IProcessBus receiveContext =
                new ProcessBus(bus, new PoolQueue(pool, this), transfer);
            receiveContext.Start();
            int totalMessages = 10000000;

            AutoResetEvent reset = new AutoResetEvent(false);
            OnMessage<int> received = delegate(IMessageHeader header, int count)
                                          {
                                              if (count == totalMessages)
                                              {
                                                  reset.Set();
                                              }
                                          };
            object topic = new object();
            TopicEquals selectall = new TopicEquals(topic);
            receiveContext.Subscribe<int>(selectall, received);

            Stopwatch watch = new Stopwatch();
            watch.Start();
            for (int i = 1; i <= totalMessages; i++)
            {
                pubContext.Publish(topic, i);
            }
            Console.WriteLine("Done publishing.");
            Assert.IsTrue(reset.WaitOne(45000, false));
            Console.WriteLine("Time: " + watch.ElapsedMilliseconds + " count: " + totalMessages);
            Console.WriteLine("Avg Per Second: " + (totalMessages/watch.Elapsed.TotalSeconds));
        }


        public static void Main(string[] args)
        {
            new PerfTests().PubSub();
        }

        private int count = 0;
        private int commandCount = 0;
        public void ExecuteAll(Command[] toExecute)
        {
            count++;
            foreach (Command command in toExecute)
            {
                command();
                commandCount++;
            }
            if(count % 1000 == 0)
            {
                Console.WriteLine("Count: " + count + " Execs: " + commandCount + " Avg: " + (commandCount/(double)count));
                count = 0;
                commandCount = 0;
            }
            Thread.Sleep(1);
        }
    }
}
