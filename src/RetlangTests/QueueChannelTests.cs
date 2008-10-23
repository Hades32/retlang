using NUnit.Framework;
using System.Threading;
using System;
using System.Collections.Generic;
using Retlang.Channels;
using Retlang.Core;
using Retlang.Fibers;

namespace RetlangTests
{
    [TestFixture]
    public class QueueChannelTests
    {

        [Test]
        public void SingleConsumer()
        {
            PoolFiber one = new PoolFiber();
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
        public void SingleConsumerWithException()
        {
            StubExecutor exec = new StubExecutor();
            PoolFiber one = new PoolFiber(new DefaultThreadPool(), exec);
            one.Start();
            AutoResetEvent reset = new AutoResetEvent(false);
            using (one)
            {
                QueueChannel<int> channel = new QueueChannel<int>();
                Action<int> onMsg = delegate(int num)
                {
                    if (num == 0)
                    {
                        throw new Exception();
                    }
                    reset.Set();
                };
                channel.Subscribe(one, onMsg);
                channel.Publish(0);
                channel.Publish(1);
                Assert.IsTrue(reset.WaitOne(10000, false));
                Assert.AreEqual(1, exec.failed.Count);
            }
        }


        [Test]
        public void Multiple()
        {
            List<IFiber> queues = new List<IFiber>();
            int receiveCount = 0;
            AutoResetEvent reset = new AutoResetEvent(false);
            QueueChannel<int> channel = new QueueChannel<int>();

            int messageCount = 100;
            object updateLock = new object();
            for (int i = 0; i < 5; i++)
            {
                Action<int> onReceive = delegate
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
                PoolFiber fiber = new PoolFiber();
                fiber.Start();
                queues.Add(fiber);
                channel.Subscribe(fiber, onReceive);
            }
            for (int i = 0; i < messageCount; i++)
            {
                channel.Publish(i);
            }
            Assert.IsTrue(reset.WaitOne(10000, false));
            queues.ForEach(delegate(IFiber q) { q.Dispose(); });
        }
    }

    public class StubExecutor : IBatchExecutor
    {
        public List<Exception> failed = new List<Exception>();

        public void ExecuteAll(Command[] toExecute)
        {
            foreach (Command c in toExecute)
            {
                try
                {
                    c();
                }
                catch (Exception e)
                {
                    failed.Add(e);
                }
            }
        }

    }
}
