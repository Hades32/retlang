using System;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;
using Retlang;

namespace RetlangTests
{
    [TestFixture]
    public class PerfTests: ICommandExecutor
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
            ProcessThreadFactory threadFactory = (ProcessThreadFactory)factory.ThreadFactory;

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
            TopicEquals selectall = new TopicEquals("string");
            receiveContext.Subscribe<int>(selectall, received);
            
            Stopwatch watch = new Stopwatch();
            watch.Start();
            for (int i = 1; i <= totalMessages; i++)
            {
                pubContext.Publish("string", i);
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
            PoolQueue busQueue = new PoolQueue(pool, new DefaultCommandExecutor());
            busQueue.Start();
            MessageBus bus = new MessageBus(busQueue);
            bus.AsyncPublish = false;
            
            ObjectTransferEnvelopeFactory transfer = new ObjectTransferEnvelopeFactory();
            IProcessContext pubContext = new ProcessContext(bus, new PoolQueue(pool, new DefaultCommandExecutor()), transfer);
            pubContext.Start();
            IProcessContext receiveContext = new ProcessContext(bus, new PoolQueue(pool, new DefaultCommandExecutor()), transfer);
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
            TopicEquals selectall = new TopicEquals("string");
            receiveContext.Subscribe<int>(selectall, received);

            Stopwatch watch = new Stopwatch();
            watch.Start();
            for (int i = 1; i <= totalMessages; i++)
            {
                pubContext.Publish("string", i);
            }
            Console.WriteLine("Done publishing.");
            Assert.IsTrue(reset.WaitOne(45000, false));
            Console.WriteLine("Time: " + watch.ElapsedMilliseconds + " count: " + totalMessages);
            Console.WriteLine("Avg Per Second: " + (totalMessages / watch.Elapsed.TotalSeconds));
        }


        public static void Main(string[] args)
        {
            new PerfTests().PubSub();
        }

        public void ExecuteAll(Command[] toExecute)
        {
            foreach (Command command in toExecute)
            {
                command();
            }
            Thread.Sleep(1);
        }
    }
}