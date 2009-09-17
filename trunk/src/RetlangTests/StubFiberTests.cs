using System;
using System.Collections.Generic;
using NUnit.Framework;
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
    }
}