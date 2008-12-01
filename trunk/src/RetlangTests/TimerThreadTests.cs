using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using NUnit.Framework;
using Retlang.Core;
using Timer=System.Timers.Timer;

namespace RetlangTests
{
    [TestFixture]
    public class TimerThreadTests
    {
        [Test]
        public void Schedule()
        {
            var queue = new SynchronousActionQueue();
            queue.Run();

            var count = 0;
            var reset = new AutoResetEvent(false);
            Action one = () => Assert.AreEqual(0, count++);
            Action two = () => Assert.AreEqual(1, count++);
            Action three = delegate
                                {
                                    Assert.AreEqual(2, count++);
                                    reset.Set();
                                };

            using (var thread = new TimerThread())
            {
                thread.Start();
                thread.Schedule(queue, three, 50);
                thread.Schedule(queue, one, 1);
                thread.Schedule(queue, two, 1);
                Assert.IsTrue(reset.WaitOne(10000, false));
            }
        }

        [Test]
        public void TimeTilNextNothingQueued()
        {
            using (var timer = new TimerThread())
            {
                timer.Start();
                long result = 0;
                Assert.IsFalse(timer.GetTimeTilNext(ref result, 100));
                Assert.AreEqual(0, result);
            }
        }

        [Test]
        public void TimeTilNext()
        {
            var queue = new SynchronousActionQueue();
            queue.Run();
            Action action = () => Assert.Fail("Should not execute");
            using (var timer = new TimerThread())
            {
                long now = 0;
                long span = 0;
                timer.QueueEvent(new SingleEvent(queue, action, 500, now));
                Assert.IsTrue(timer.GetTimeTilNext(ref span, 0));
                Assert.AreEqual(500, span);
                Assert.IsTrue(timer.GetTimeTilNext(ref span, 499));
                Assert.AreEqual(1, span);
                Assert.IsFalse(timer.GetTimeTilNext(ref span, 500));
                Assert.AreEqual(0, span);
            }
        }

        [Test]
        public void Schedule1000In1ms()
        {
            var queue = new SynchronousActionQueue();
            queue.Run();

            var count = 0;
            var reset = new AutoResetEvent(false);
            Action one = delegate
                              {
                                  count++;
                                  if (count == 1000)
                                  {
                                      reset.Set();
                                  }
                              };

            using (var thread = new TimerThread())
            {
                thread.Start();
                for (var i = 0; i < 1000; i++)
                {
                    thread.Schedule(queue, one, i);
                }
                Assert.IsTrue(reset.WaitOne(1200, false));
            }
        }

        [Test]
        [Explicit]
        public void WaitForObject()
        {
            var count = 0;
            var waiter = new AutoResetEvent(false);
            var reset = new AutoResetEvent(false);
            var stop = new Stopwatch();
            stop.Start();
            WaitOrTimerCallback callback = delegate
                                               {
                                                   if (count < 5000)
                                                   {
                                                       count++;
                                                       if (count == 5000)
                                                       {
                                                           reset.Set();
                                                       }
                                                       if (count%100 == 0)
                                                       {
                                                           Console.WriteLine(count + "in: " + stop.ElapsedMilliseconds +
                                                                             " avg: "
                                                                             +
                                                                             (stop.ElapsedMilliseconds/(double) count));
                                                       }
                                                   }
                                               };
            var regHandle = ThreadPool.RegisterWaitForSingleObject(waiter, callback, null, 5, false);
            Assert.IsTrue(reset.WaitOne(30000, false));
            regHandle.Unregister(waiter);
            stop.Stop();
            Assert.AreEqual(5000, count);
        }


        [Test]
        [Explicit]
        public void TimerInterval()
        {
            var timer = new Timer();
            timer.Interval = 1000;
            timer.AutoReset = true;
            var count = 0;
            var reset = new AutoResetEvent(false);
            var stop = new Stopwatch();
            stop.Start();
            timer.Elapsed += delegate
                                 {
                                     if (count < 5000)
                                     {
                                         count++;
                                         if (count == 5000)
                                         {
                                             reset.Set();
                                         }
                                         if (count%2 == 0)
                                         {
                                             Console.WriteLine(count + "in: " + stop.ElapsedMilliseconds + " avg: "
                                                               + ((double) stop.ElapsedMilliseconds/(double) count));
                                         }
                                     }
                                 };
            timer.Start();
            Assert.IsTrue(reset.WaitOne(30000, false));
            timer.Stop();
            stop.Stop();
            Assert.AreEqual(5000, count);
        }

        [Test, Explicit]
        public void ScheduleOn1MsInterval()
        {
            var caps = new TIMECAPS();
            PerfSettings.timeGetDevCaps(ref caps, Marshal.SizeOf(caps));
            Console.WriteLine(caps.PeriodMin + "-" + caps.PeriodMax);
            var queue = new SynchronousActionQueue();
            queue.Run();

            var count = 0;
            var reset = new AutoResetEvent(false);
            Action one = delegate
                              {
                                  count++;
                                  if (count == 1000)
                                  {
                                      reset.Set();
                                  }
                              };

            using (var thread = new TimerThread())
            {
                thread.Start();
                thread.ScheduleOnInterval(queue, one, 1, 1);
                Assert.IsTrue(reset.WaitOne(10000, false));
            }
        }
    }
}