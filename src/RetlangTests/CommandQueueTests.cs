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
            var repo = new MockRepository();
            var excCommand = repo.CreateMock<Action>();
            var failure = new Exception();
            excCommand();
            LastCall.Throw(failure);

            repo.ReplayAll();

            var queue = new CommandQueue();
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
        
        [Test]
        public void ShouldOnlyExecuteCommandsQueuedWhileNotStopped()
        {
            var mockery = new MockRepository();
            var command1 = mockery.CreateMock<Action>();
            var command2 = mockery.CreateMock<Action>();
            var command3 = mockery.CreateMock<Action>();

            using (mockery.Record())
            {
                command1();
                command2();
            }


            using (mockery.Playback())
            {
                var queue = new CommandQueue();
                queue.Enqueue(command1);

                var run = new Thread(queue.Run);

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
            var queue = new CommandQueue();
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