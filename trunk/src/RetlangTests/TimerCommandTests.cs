using System;
using System.Threading;
using NUnit.Framework;
using Retlang.Core;
using Rhino.Mocks;

namespace RetlangTests
{
    [TestFixture]
    public class TimerCommandTests
    {
        [Test]
        public void Cancel()
        {
            var executionCount = 0;
            Action com = delegate { executionCount++; };
            var timer = new TimerCommand(com, 1, 2);
            timer.ExecuteOnProcessThread();
            Assert.AreEqual(1, executionCount);
            timer.Cancel();
            timer.ExecuteOnProcessThread();

            Assert.AreEqual(1, executionCount);
        }

        [Test]
        public void CallbackFromTimer()
        {
            var mocks = new MockRepository();

            var command = mocks.CreateMock<Action>();
            var timer = new TimerCommand(command, 2, 3);
            var registry = mocks.CreateMock<IPendingCommandRegistry>();
            registry.EnqueueTask(timer.ExecuteOnProcessThread);

            mocks.ReplayAll();

            timer.ExecuteOnTimerThread(registry);
        }

        [Test]
        public void CallbackFromIntervalTimerWithCancel()
        {
            var mocks = new MockRepository();
            var command = mocks.CreateMock<Action>();
            var timer = new TimerCommand(command, 2, 3);
            var registry = mocks.CreateMock<IPendingCommandRegistry>();

            registry.Remove(timer);

            mocks.ReplayAll();

            timer.Cancel();
            timer.ExecuteOnTimerThread(registry);
        }

        [Test]
        public void CallbackFromTimerWithCancel()
        {
            var mocks = new MockRepository();
            var command = mocks.CreateMock<Action>();
            var timer = new TimerCommand(command, 2, Timeout.Infinite);
            var registry = mocks.CreateMock<IPendingCommandRegistry>();

            registry.Remove(timer);
            registry.EnqueueTask(timer.ExecuteOnProcessThread);

            mocks.ReplayAll();
            timer.ExecuteOnTimerThread(registry);
        }
    }
}