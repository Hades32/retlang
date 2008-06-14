using NUnit.Framework;
using Retlang;
using System.Threading;
using System;
using System.Collections.Generic;

namespace RetlangTests
{
    [TestFixture]
    public class QueueChannelTests
    {

        [Test]
        public void SingleConsumer()
        {
            PoolQueue one = new PoolQueue();
            one.Start();
            int oneConsumed = 0;
            AutoResetEvent reset = new AutoResetEvent(false);
            using (one)
            {
                QueueChannel<int> channel = new QueueChannel<int>();
                Action<int> onMsg = delegate
                {
                    oneConsumed++;
                    if (oneConsumed == 20)
                    {
                        reset.Set();
                    }
                };
                channel.Subscribe(one, onMsg);
                for (int i = 0; i < 20; i++)
                {
                    channel.Publish(i);
                }
                Assert.IsTrue(reset.WaitOne(10000, false));
            }
        }

        [Test]
        public void Multiple()
        {
            List<IProcessQueue> queues = new List<IProcessQueue>();
            int receiveCount = 0;
            AutoResetEvent reset = new AutoResetEvent(false);
            QueueChannel<int> channel = new QueueChannel<int>();

            int messageCount = 100;
            object updateLock = new object();
            for (int i = 0; i < 5; i++)
            {
                Action<int> onReceive = delegate(int msgNum)
                {
                    Thread.Sleep(15);
                    lock (updateLock)
                    {
                        receiveCount++;
                        if (receiveCount == messageCount)
                        {
                            reset.Set();
                        }
                    }
                };
                PoolQueue queue = new PoolQueue();
                queue.Start();
                queues.Add(queue);
                channel.Subscribe(queue, onReceive);
            }
            for (int i = 0; i < messageCount; i++)
            {
                channel.Publish(i);
            }
            Assert.IsTrue(reset.WaitOne(10000, false));
            queues.ForEach(delegate(IProcessQueue q) { q.Stop(); });

        }

    }
}
