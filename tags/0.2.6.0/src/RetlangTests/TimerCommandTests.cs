using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Retlang;
using Rhino.Mocks;

namespace RetlangTests
{

    [TestFixture]
    public class TimerCommandTests
    {

        [Test]
        public void Cancel()
        {
            int executionCount = 0;
            Command com = delegate { executionCount++; };
            TimerCommand timer = new TimerCommand(com, 1, 2);
            timer.ExecuteOnProcessThread();
            Assert.AreEqual(1, executionCount);
            timer.Cancel();
            timer.ExecuteOnProcessThread();

            Assert.AreEqual(1, executionCount);

        }

        [Test]
        public void CallbackFromTimer()
        {
            MockRepository mocks = new MockRepository();

            Command command = mocks.CreateMock<Command>();
            TimerCommand timer = new TimerCommand(command, 2, 3);
            IPendingCommandRegistry registry = mocks.CreateMock<IPendingCommandRegistry>();
            registry.EnqueueTask(timer.ExecuteOnProcessThread);

            mocks.ReplayAll();
            
            timer.ExecuteOnTimerThread(registry);
        }

        [Test]
        public void CallbackFromIntervalTimerWithCancel()
        {
            MockRepository mocks = new MockRepository();
            Command command = mocks.CreateMock<Command>();
            TimerCommand timer = new TimerCommand(command, 2, 3);
            IPendingCommandRegistry registry = mocks.CreateMock<IPendingCommandRegistry>();
            
            registry.Remove(timer);

            mocks.ReplayAll();

            timer.Cancel();
            timer.ExecuteOnTimerThread(registry);
        }

        [Test]
        public void CallbackFromTimerWithCancel()
        {
            MockRepository mocks = new MockRepository();
            Command command = mocks.CreateMock<Command>();
            TimerCommand timer = new TimerCommand(command, 2, Timeout.Infinite);
            IPendingCommandRegistry registry = mocks.CreateMock<IPendingCommandRegistry>();

            registry.Remove(timer);
            registry.EnqueueTask(timer.ExecuteOnProcessThread);

            mocks.ReplayAll();
            timer.ExecuteOnTimerThread(registry);
        }


    }
}
