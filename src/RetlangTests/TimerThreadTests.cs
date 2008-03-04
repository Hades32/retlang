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
                TimeSpan result = TimeSpan.Zero;
                Assert.IsFalse(timer.GetTimeTilNext(ref result, DateTime.Now));
                Assert.AreEqual(TimeSpan.Zero, result);
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
                DateTime now = DateTime.Now;
                TimeSpan span = TimeSpan.Zero;
                timer.QueueEvent(new SingleEvent(queue, command, 500, now));
                Assert.IsTrue(timer.GetTimeTilNext(ref span, now));
                Assert.AreEqual(500, span.TotalMilliseconds);
                Assert.IsTrue(timer.GetTimeTilNext(ref span, now.AddMilliseconds(499)));
                Assert.AreEqual(1, span.TotalMilliseconds);
                Assert.IsFalse(timer.GetTimeTilNext(ref span, now.AddMilliseconds(500)));
                Assert.AreEqual(0, span.TotalMilliseconds);
  
            }
        }

        [Test]
        public void ScheduleOnInterval()
        {
            SynchronousCommandQueue queue = new SynchronousCommandQueue();
            queue.Run();

            int count = 0;
            AutoResetEvent reset = new AutoResetEvent(false);
            Command one = delegate
                              {
                                  count++;
                                  if(count == 10)
                                  {
                                      reset.Set();
                                  }
                              };

            using (TimerThread thread = new TimerThread())
            {
                thread.Start();
                thread.ScheduleOnInterval(queue, one, 1, 1);
                Assert.IsTrue(reset.WaitOne(20000, false));
            }

        }
    }
}
