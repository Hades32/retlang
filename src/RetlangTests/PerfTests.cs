using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Infinite loop to check memory performance with scheduling. 
        /// </summary>
        [Test]
        [Explicit]
        public void TimerMemoryTest()
        {
            ProcessContextFactory factory = ProcessFactoryFixture.CreateAndStart();

            IProcessContext pubContext = CreateContext(factory);
            while (true)
            {
                pubContext.Schedule(delegate { }, 1);
                //Thread.Sleep(1);
            }
        }

        [Test]
        [Explicit]
        public void TimerMemoryTestWithTimerThread()
        {
            ProcessContextFactory factory = ProcessFactoryFixture.CreateAndStart();

            IProcessContext pubContext = CreateContext(factory);
            using (TimerThread timer = new TimerThread())
            {
                timer.Start();
                while (true)
                {
                    timer.Schedule(pubContext, delegate { }, 1);
                    Thread.Sleep(1);
                }
            }
        }

        [Test]
        [Explicit]
        public void TimerTestShortInterval()
        {
            ProcessContextFactory factory = ProcessFactoryFixture.CreateAndStart();

            IProcessContext pubContext = CreateContext(factory);
            using (TimerThread timer = new TimerThread())
            {
                timer.Start();
                timer.ScheduleOnInterval(pubContext, delegate { }, 50, 50);
                Thread.Sleep(Timeout.Infinite);
            }
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

        [Test]
        [Explicit]
        public void Multisubscriber()
        {
            int topicCount = 10;
            int maxCount = 1000000;
            ProcessContextFactory factory = new ProcessContextFactory();
            factory.Start();
            MessageBus bus = (MessageBus) factory.MessageBus;
            bus.AsyncPublish = false;

            List<object> topics = new List<object>();
            for (int i = 0; i < topicCount; i++)
            {
                topics.Add(new object());
            }

            List<IProcessContext> contexts = new List<IProcessContext>();
            List<AutoResetEvent> monitors = new List<AutoResetEvent>();
            for (int i = 0; i < topicCount; i++)
            {
                IProcessContext context = factory.CreateAndStart();
                AutoResetEvent reset = new AutoResetEvent(false);
                int contextCount = 0;
                OnMessage<string> onMessage = delegate
                                                  {
                                                      contextCount++;
                                                      if (maxCount == contextCount)
                                                      {
                                                          Console.WriteLine("count: " + contextCount);
                                                          reset.Set();
                                                      }
                                                  };
                context.Subscribe(new TopicEquals(topics[i]), onMessage);
                contexts.Add(factory.CreateAndStart());
                monitors.Add(reset);
            }

            Stopwatch watch = Stopwatch.StartNew();
            for (int i = 0; i < topicCount; i++)
            {
                object topic = topics[i];
                for (int p = 0; p < maxCount; p++)
                    factory.Publish(topic, "data");
            }
            foreach (AutoResetEvent monitor in monitors)
            {
                Assert.IsTrue(monitor.WaitOne(30000, false));
            }
            Console.WriteLine("Time: " + watch.ElapsedMilliseconds + " count: " + maxCount*topicCount);
            Console.WriteLine("Avg Per Second: " + (maxCount*topicCount/watch.Elapsed.TotalSeconds));
            //Console.WriteLine("Avg: " + );
        }


        public static void Main(string[] args)
        {
            PerfTests tests = new PerfTests();
            Console.WriteLine("PubSub with Threads");
            tests.PubSub();

            Console.WriteLine("Pub Sub With Pool");
            tests.PubSubWithPool();
        }

        private int count = 0;
        private int commandCount = 0;
        private DateTime startTime = DateTime.Now;

        public void ExecuteAll(Command[] toExecute)
        {
            count++;
            foreach (Command command in toExecute)
            {
                command();
                commandCount++;
            }
            if (commandCount > 500000)
            {
                Console.WriteLine("Count: " + count + " Execs: " + commandCount + " Avg: " +
                                  (commandCount/(double) count));
                Console.WriteLine("Rate: " + (DateTime.Now - startTime).TotalMilliseconds/commandCount);
                count = 0;
                commandCount = 0;
                startTime = DateTime.Now;
            }
            Thread.Sleep(1);
        }
    }
}