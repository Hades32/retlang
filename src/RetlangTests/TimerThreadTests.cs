using System;
using System.Threading;
using NUnit.Framework;
using Retlang;

namespace RetlangTests
{
    [TestFixture]
    public class TimerThreadTests
    {
        [Test]
        public void Schedule()
        {
            SynchronousCommandQueue queue = new SynchronousCommandQueue();
            queue.Run();

            int count = 0;
            AutoResetEvent reset = new AutoResetEvent(false);
            Command one = delegate
                              {
                                  Assert.AreEqual(0, count++);
                              };
            Command two = delegate
                              {
                                  Assert.AreEqual(1, count++);
                              };
            Command three = delegate
                                {
                                    Assert.AreEqual(2, count++);
                                    reset.Set();
                                };

            using (TimerThread thread = new TimerThread())
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
            using (TimerThread timer = new TimerThread())
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
            SynchronousCommandQueue queue = new SynchronousCommandQueue();
            queue.Run();
            Command command = delegate{ Assert.Fail("Should not execute");};
            using (TimerThread timer = new TimerThread())
            {
                long now = 0;
                long span = 0;
                timer.QueueEvent(new SingleEvent(queue, command, 500, now));
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
            SynchronousCommandQueue queue = new SynchronousCommandQueue();
            queue.Run();

            int count = 0;
            AutoResetEvent reset = new AutoResetEvent(false);
            Command one = delegate
                              {
                                  count++;
                                  if(count == 1000)
                                  {
                                      reset.Set();
                                  }
                              };

            using (TimerThread thread = new TimerThread())
            {
                thread.Start();
                for (int i = 0; i < 1000; i++)
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
            int count = 0;
            AutoResetEvent waiter = new AutoResetEvent(false);
            AutoResetEvent reset = new AutoResetEvent(false);
            System.Diagnostics.Stopwatch stop = new System.Diagnostics.Stopwatch();
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
                    if (count % 100 == 0)
                    {
                        Console.WriteLine(count + "in: " + stop.ElapsedMilliseconds + " avg: "
                            + ((double)stop.ElapsedMilliseconds / (double)count));
                    }
                }
            };
            RegisteredWaitHandle regHandle = ThreadPool.RegisterWaitForSingleObject(waiter, callback, null, 5, false);
            Assert.IsTrue(reset.WaitOne(30000, false));
            regHandle.Unregister(waiter);
            stop.Stop();
            Assert.AreEqual(5000, count);
        }



        [Test]
        [Explicit]
        public void TimerInterval()
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 1000;
            timer.AutoReset = true;
            int count = 0;
            AutoResetEvent reset = new AutoResetEvent(false);
            System.Diagnostics.Stopwatch stop = new System.Diagnostics.Stopwatch();
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
                    if (count % 2 == 0)
                    {
                        Console.WriteLine(count + "in: " + stop.ElapsedMilliseconds + " avg: " 
                            + ((double)stop.ElapsedMilliseconds/(double)count));
                    }
                }
            };
            timer.Start();
            Assert.IsTrue(reset.WaitOne(30000, false));
            timer.Stop();
            stop.Stop();
            Assert.AreEqual(5000, count);
        }

        [Test]
        public void ScheduleOn1MsInterval()
        {
            SynchronousCommandQueue queue = new SynchronousCommandQueue();
            queue.Run();

            int count = 0;
            AutoResetEvent reset = new AutoResetEvent(false);
            Command one = delegate
                              {
                                  count++;
                                  if (count == 10)
                                  {
                                      reset.Set();
                                  }
                              };

            using (TimerThread thread = new TimerThread())
            {
                thread.Start();
                thread.ScheduleOnInterval(queue, one, 100, 100);
                Assert.IsTrue(reset.WaitOne(1100, false));
            }

        }
    }
}
