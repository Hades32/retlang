using System;
using System.Collections.Generic;
using NUnit.Framework;
using Retlang;
using Rhino.Mocks;

namespace RetlangTests
{
    [TestFixture]
    public class CommandQueueTests
    {

        [Test]
        public void NoExceptionHandling()
        {
            MockRepository repo = new MockRepository();
            OnCommand excCommand = repo.CreateMock<OnCommand>();
            Exception failure = new Exception();
            excCommand();
            LastCall.Throw(failure);

            repo.ReplayAll();

            CommandQueue queue = new CommandQueue();
            queue.Enqueue(excCommand);

            try
            {
                queue.ExecuteNext();
                Assert.Fail("Should throw Exception");
            }
            catch (Exception commFailure)
            {
                Assert.AreSame(failure, commFailure);
            }
            repo.VerifyAll();
        }

        [Test]
        public void ExceptionHandling()
        {
            MockRepository repo = new MockRepository();
            OnCommand excCommand = repo.CreateMock<OnCommand>();
            Exception failure = new Exception();
            excCommand();
            LastCall.Throw(failure);

            OnException handler = repo.CreateMock<OnException>();
            handler(excCommand, failure);

            repo.ReplayAll();

            CommandQueue queue = new CommandQueue();
            queue.ExceptionEvent += handler;
            queue.Enqueue(excCommand);

            queue.ExecuteNext();

            repo.VerifyAll();
        }

    }
}
