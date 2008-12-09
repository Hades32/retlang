using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
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
            var one = new PoolFiber();
            one.Start();
            var oneConsumed = 0;
            var reset = new AutoResetEvent(false);
            using (one)
            {
                var channel = new QueueChannel<int>();
                Action<int> onMsg = delegate
                {
                    oneConsumed++;
                    if (oneConsumed == 20)
                    {
                        reset.Set();
                    }
                };
                channel.Subscribe(one, onMsg);
                for (var i = 0; i < 20; i++)
                {
                    channel.Publish(i);
                }
                Assert.IsTrue(reset.WaitOne(10000, false));
            }
        }

        [Test]
        public void SingleConsumerWithException()
        {
            var exec = new StubExecutor();
            var one = new PoolFiber(new DefaultThreadPool(), exec);
            one.Start();
            var reset = new AutoResetEvent(false);
            using (one)
            {
                var channel = new QueueChannel<int>();
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
            var queues = new List<IFiber>();
            var receiveCount = 0;
            var reset = new AutoResetEvent(false);
            var channel = new QueueChannel<int>();

            var messageCount = 100;
            var updateLock = new object();
            for (var i = 0; i < 5; i++)
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
                var fiber = new PoolFiber();
                fiber.Start();
                queues.Add(fiber);
                channel.Subscribe(fiber, onReceive);
            }
            for (var i = 0; i < messageCount; i++)
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

        public void ExecuteAll(Action[] toExecute)
        {
            foreach (var action in toExecute)
            {
                Execute(action);
            }
        }

        public void Execute(Action toExecute)
        {
            try
            {
                toExecute();
            }
            catch (Exception e)
            {
                failed.Add(e);
            }
        }
    }
}
