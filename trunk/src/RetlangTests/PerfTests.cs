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
            if(toExecute.Length < 1000)
                Thread.Sleep(1);
        }

    }

    [TestFixture]
    public class PerfTests
    {
        [Test, Explicit]
        public void PointToPointPerfTest()
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
                    Assert.IsTrue(reset.WaitOne(30000, false));
                }
            }
        }

    }
}