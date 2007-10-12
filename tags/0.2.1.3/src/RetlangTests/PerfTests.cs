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
        [Test]
        [Explicit]
        public void PubSub()
        {
            int totalMessages = 10000000;
            ProcessContextFactory factory = ProcessFactoryFixture.CreateAndStart();
            ProcessThreadFactory threadFactory = (ProcessThreadFactory)factory.ThreadFactory;
            threadFactory.Executor = this;

            IProcessContext pubContext = factory.CreateAndStart();
            IProcessContext receiveContext = factory.CreateAndStart();

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