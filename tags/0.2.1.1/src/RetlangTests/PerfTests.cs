using System;
using System.Diagnostics;
using NUnit.Framework;
using Retlang;

namespace RetlangTests
{
    [TestFixture]
    public class PerfTests
    {
        [Test]
        [Explicit]
        public void PubSub()
        {
            int totalMessages = 10000000;
            ProcessContextFactory factory = ProcessFactoryFixture.CreateAndStart();

            IProcessContext pubContext = factory.CreateAndStart();
            IProcessContext receiveContext = factory.CreateAndStart();

            OnMessage<int> received = delegate(IMessageHeader header, int count)
                                          {
                                              if (count == totalMessages)
                                              {
                                                  receiveContext.Stop();
                                              }
                                          };
            receiveContext.Subscribe<int>(new TopicEquals("sub"), received);

            Stopwatch watch = new Stopwatch();
            watch.Start();
            for (int i = 1; i <= totalMessages; i++)
            {
                pubContext.Publish("sub", i);
            }
            receiveContext.Join();
            pubContext.Stop();
            pubContext.Join();
            factory.Stop();
            factory.Join();

            Console.WriteLine("Time: " + watch.ElapsedMilliseconds + " count: " + totalMessages);
            Console.WriteLine("Avg Per Second: " + (totalMessages/watch.Elapsed.TotalSeconds));
        }
    }
}