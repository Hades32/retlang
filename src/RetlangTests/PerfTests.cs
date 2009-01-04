using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;
using Retlang.Fibers;
using Retlang.Channels;
using Retlang.Core;

namespace RetlangTests
{
    public class PerfExecutor : IBatchExecutor
    {
        public void ExecuteAll(Action[] toExecute)
        {
            for (int i = 0; i < toExecute.Length; i++)
            {
                toExecute[i]();
            }
            if (toExecute.Length < 10000)
            {
                Thread.Sleep(1);
                //Thread.SpinWait(100);
                //Console.WriteLine("num: " + toExecute.Length);
            }
        }

    }
    public struct MsgStruct
    {
        public int count;
        public long other;
    }

    [TestFixture]
    public class PerfTests
    {
        [Test, Explicit]
        public void PointToPointPerfTestWithStruct()
        {
            ActionExecutor executor = new ActionExecutor();
            executor.BatchExecutor = new PerfExecutor();
            executor.MaxDepth = 10000;
            executor.MaxEnqueueWaitTime = 1000;
            using (IFiber bus = new ThreadFiber(executor))
            {
                bus.Start();
                IChannel<MsgStruct> channel = new Channel<MsgStruct>();
                int max = 5000000;
                AutoResetEvent reset = new AutoResetEvent(false);
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
                    for (int i = 0; i <= max; i++)
                    {
                        MsgStruct msg = new MsgStruct();
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
            ActionExecutor executor = new ActionExecutor();
            executor.BatchExecutor = new PerfExecutor();
            executor.MaxDepth = 10000;
            executor.MaxEnqueueWaitTime = 1000;
            using (IFiber bus = new ThreadFiber(executor))
            {
                bus.Start();
                IChannel<int> channel = new Channel<int>();
                int max = 5000000;
                AutoResetEvent reset = new AutoResetEvent(false);
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
                    for (int i = 0; i <= max; i++)
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
            ActionExecutor executor = new ActionExecutor();
            executor.BatchExecutor = new PerfExecutor();
            executor.MaxDepth = 100000;
            executor.MaxEnqueueWaitTime = 1000;
            using (IFiber bus = new ThreadFiber(executor))
            {
                bus.Start();
                IChannel<object> channel = new Channel<object>();
                int max = 5000000;
                AutoResetEvent reset = new AutoResetEvent(false);
                object end = new object();
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
                    object msg = new object();
                    for (int i = 0; i <= max; i++)
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