using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Retlang.Channels;
using Retlang.Core;
using Retlang.Fibers;

namespace RetlangTests
{
    public class PerfExecutor : IBatchExecutor
    {
        public void ExecuteAll(List<Action> toExecute)
        {
            for (var i = 0; i < toExecute.Count; i++)
            {
                toExecute[i]();
            }
            if (toExecute.Count < 10000)
            {
                Thread.Sleep(1);
            }
        }
    }

    public struct MsgStruct
    {
        public int count;
    }

    [TestFixture]
    public class PerfTests
    {
        [Test, Explicit]
        public void PointToPointPerfTestWithStruct()
        {
            var executor = new ActionExecutor();
            executor.BatchExecutor = new PerfExecutor();
            executor.MaxDepth = 10000;
            executor.MaxEnqueueWaitTime = 1000;
            using (IFiber bus = new ThreadFiber(executor))
            {
                bus.Start();
                IChannel<MsgStruct> channel = new Channel<MsgStruct>();
                var max = 5000000;
                var reset = new AutoResetEvent(false);
                Action<MsgStruct> onMsg = delegate(MsgStruct count)
                                              {
                                                  if (count.count == max)
                                                  {
                                                      reset.Set();
                                                  }
                                              };
                channel.Subscribe(bus, onMsg);
                using (new PerfTimer(max))
                {
                    for (var i = 0; i <= max; i++)
                    {
                        var msg = new MsgStruct();
                        msg.count = i;
                        channel.Publish(msg);
                    }
                    Console.WriteLine("done pub");
                    Assert.IsTrue(reset.WaitOne(30000, false));
                }
            }
        }

        [Test, Explicit]
        public void PointToPointPerfTestWithInt()
        {
            var executor = new ActionExecutor();
            executor.BatchExecutor = new PerfExecutor();
            executor.MaxDepth = 10000;
            executor.MaxEnqueueWaitTime = 1000;
            using (IFiber bus = new ThreadFiber(executor))
            {
                bus.Start();
                IChannel<int> channel = new Channel<int>();
                var max = 5000000;
                var reset = new AutoResetEvent(false);
                Action<int> onMsg = delegate(int count)
                                        {
                                            if (count == max)
                                            {
                                                reset.Set();
                                            }
                                        };
                channel.Subscribe(bus, onMsg);
                using (new PerfTimer(max))
                {
                    for (var i = 0; i <= max; i++)
                    {
                        channel.Publish(i);
                    }
                    Console.WriteLine("done pub");
                    Assert.IsTrue(reset.WaitOne(30000, false));
                }
            }
        }

        [Test, Explicit]
        public void PointToPointPerfTestWithObject()
        {
            var executor = new ActionExecutor();
            executor.BatchExecutor = new PerfExecutor();
            executor.MaxDepth = 100000;
            executor.MaxEnqueueWaitTime = 1000;
            using (IFiber bus = new ThreadFiber(executor))
            {
                bus.Start();
                IChannel<object> channel = new Channel<object>();
                var max = 5000000;
                var reset = new AutoResetEvent(false);
                var end = new object();
                Action<object> onMsg = delegate(object msg)
                                           {
                                               if (msg == end)
                                               {
                                                   reset.Set();
                                               }
                                           };
                channel.Subscribe(bus, onMsg);
                using (new PerfTimer(max))
                {
                    var msg = new object();
                    for (var i = 0; i <= max; i++)
                    {
                        channel.Publish(msg);
                    }
                    channel.Publish(end);
                    Console.WriteLine("done pub");
                    Assert.IsTrue(reset.WaitOne(30000, false));
                }
            }
        }
    }
}