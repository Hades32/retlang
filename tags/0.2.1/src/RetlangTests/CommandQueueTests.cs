using System;
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