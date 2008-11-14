using System;
using System.Threading;
using NUnit.Framework;
using Retlang.Core;
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
            Command excCommand = repo.CreateMock<Command>();
            Exception failure = new Exception();
            excCommand();
            LastCall.Throw(failure);

            repo.ReplayAll();

            CommandQueue queue = new CommandQueue();
            queue.Enqueue(excCommand);

            try
            {
                queue.ExecuteNextBatch();
                Assert.Fail("Should throw Exception");
            }
            catch (Exception commFailure)
            {
                Assert.AreSame(failure, commFailure);
            }
            repo.VerifyAll();
        }

        private class Check
        {
            public Check()
            {
            }
        }

        [Test]
        public void ShouldOnlyExecuteCommandsQueuedWhileNotStopped()
        {
            MockRepository mockery = new MockRepository();
            Command command1 = mockery.CreateMock<Command>();
            Command command2 = mockery.CreateMock<Command>();
            Command command3 = mockery.CreateMock<Command>();

            using (mockery.Record())
            {
                command1();
                command2();
            }


            using (mockery.Playback())
            {
                CommandQueue queue = new CommandQueue();
                queue.Enqueue(command1);

                Thread run = new Thread(new ThreadStart(
                                            delegate { queue.Run(); }));

                run.Start();
                Thread.Sleep(100);
                queue.Enqueue(command2);
                queue.Stop();
                queue.Enqueue(command3);
                Thread.Sleep(100);
                run.Join();
            }
        }

        [Test]
        public void MaxDepth()
        {
            CommandQueue queue = new CommandQueue();
            queue.MaxDepth = 2;
            queue.Enqueue(delegate { });
            queue.Enqueue(delegate { });

            try
            {
                queue.Enqueue(delegate { });
                Assert.Fail("failed");
            }
            catch (QueueFullException failed)
            {
                Assert.AreEqual(2, failed.Depth);
                Assert.AreEqual("Attempted to enqueue item into full queue: 2", failed.Message);
            }
        }
    }
}