using System;
using System.Collections.Generic;
using NUnit.Framework;
using Retlang.Channels;
using Retlang.Fibers;

namespace RetlangTests
{
    [TestFixture]
    public class StubFiberTests
    {
        [Test]
        public void StubFiberPendingTasksShouldAllowEnqueueOfCommandsWhenExecutingAllPending()
        {
            var fiber = new StubFiber { ExecutePendingImmediately = false };

            var fired1 = new object();
            var fired2 = new object();
            var fired3 = new object();

            var actionMarkers = new List<object>();

            Action command1 = delegate
                                  {
                                      actionMarkers.Add(fired1);
                                      fiber.Enqueue(() => actionMarkers.Add(fired3));
                                  };

            Action command2 = () => actionMarkers.Add(fired2);

            fiber.Enqueue(command1);
            fiber.Enqueue(command2);

            fiber.ExecuteAllPendingUntilEmpty();
            Assert.AreEqual(new[] { fired1, fired2, fired3 }, actionMarkers.ToArray());
        }

        [Test]
        public void ScheduledTasksShouldBeExecutedOnceScheduleIntervalShouldBeExecutedEveryTimeExecuteScheduleAllIsCalled()
        {
            var fiber = new StubFiber();

            var scheduleFired = 0;
            var scheduleOnIntervalFired = 0;

            fiber.Schedule(() => scheduleFired++, 100);
            var intervalSub = fiber.ScheduleOnInterval(() => scheduleOnIntervalFired++, 100, 100);

            fiber.ExecuteAllScheduled();
            Assert.AreEqual(1, scheduleFired);
            Assert.AreEqual(1, scheduleOnIntervalFired);

            fiber.ExecuteAllScheduled();
            Assert.AreEqual(1, scheduleFired);
            Assert.AreEqual(2, scheduleOnIntervalFired);

            intervalSub.Cancel();

            fiber.ExecuteAllScheduled();
            Assert.AreEqual(1, scheduleFired);
            Assert.AreEqual(2, scheduleOnIntervalFired);
        }

        [Test]
        public void ShouldCompletelyClearPendingActionsBeforeExecutingNewActions()
        {
            var events = new List<int>();

            var fiber = new StubFiber { ExecutePendingImmediately = true };
            var channel = new Channel<int>();
            const int count = 4;

            channel.Subscribe(fiber, delegate(int x)
                                         {
                                             if (x == count)
                                             {
                                                 return;
                                             }

                                             channel.Publish(x + 1);
                                             events.Add(x);
                                         });

            channel.Publish(0);

            Assert.AreEqual(count, events.Count);
            for (var i = 0; i < events.Count; i++)
            {
                Assert.AreEqual(i, events[i]);
            }
        }
    }
}