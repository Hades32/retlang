using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Retlang.Channels;
using Retlang.Fibers;

namespace RetlangTests.Examples
{
    [TestFixture]
    public class BasicExamples
    {
        [Test]
        public void batching()
        {
            using (IFiber fiber = new ThreadFiber())
            {
                fiber.Start();
                var counter = new Channel<int>();
                var reset = new ManualResetEvent(false);
                int total = 0;
                Action<IList<int>> cb = delegate(IList<int> batch)
                                            {
                                                total += batch.Count;
                                                if (total == 10)
                                                {
                                                    reset.Set();
                                                }
                                            };

                counter.SubscribeToBatch(fiber, cb, 1);

                for (int i = 0; i < 10; i++)
                {
                    counter.Publish(i);
                }

                Assert.IsTrue(reset.WaitOne(10000));
            }
        }

        [Test]
        public void batchingWithKey()
        {
            using (IFiber fiber = new ThreadFiber())
            {
                fiber.Start();
                var counter = new Channel<int>();
                var reset = new ManualResetEvent(false);
                Action<IDictionary<String, int>> cb = delegate(IDictionary<String, int> batch)
                {
                    if (batch.ContainsKey("9"))
                    {
                        reset.Set();
                    }
                };

                Converter<int, String> keyResolver = x => x.ToString();
                counter.SubscribeToKeyedBatch(fiber, keyResolver, cb, 0);

                for (int i = 0; i < 10; i++)
                {
                    counter.Publish(i);
                }

                Assert.IsTrue(reset.WaitOne(10000));
            }
        }


        [Test]
        public void pubSubWithPool()
        {
            using (IFiber fiber = new PoolFiber())
            {
                fiber.Start();
                var channel = new Channel<string>();

                var reset = new AutoResetEvent(false);
                channel.Subscribe(fiber, delegate { reset.Set(); });
                channel.Publish("hello");

                Assert.IsTrue(reset.WaitOne(5000));
            }
        }

        [Test]
        public void pubSubWithThread()
        {
            using (IFiber fiber = new ThreadFiber())
            {
                fiber.Start();
                var channel = new Channel<string>();

                var reset = new AutoResetEvent(false);
                channel.Subscribe(fiber, delegate { reset.Set(); });
                channel.Publish("hello");

                Assert.IsTrue(reset.WaitOne(5000));
            }
        }

        [Test]
        public void requestReply()
        {
            using (IFiber fiber = new PoolFiber())
            {
                fiber.Start();
                var channel = new RequestReplyChannel<string, string>();
                channel.Subscribe(fiber, req => req.SendReply("bye"));
                IReply<String> reply = channel.SendRequest("hello");

                string result;
                Assert.IsTrue(reply.Receive(10000, out result));
                Assert.AreEqual("bye", result);
            }
        }

    }
}