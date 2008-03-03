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
        public void TimeTilNext()
        {
            using (TimerThread timer = new TimerThread())
            {
                timer.Start();
                TimeSpan result = TimeSpan.Zero;
                Assert.IsFalse(timer.GetTimeTilNext(ref result));
                Assert.AreEqual(TimeSpan.Zero, result);
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
