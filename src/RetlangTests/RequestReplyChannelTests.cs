using System;
using System.Threading;
using NUnit.Framework;
using Retlang.Channels;
using Retlang.Fibers;

namespace RetlangTests
{
    [TestFixture]
    public class RequestReplyChannelTests
    {
        [Test]
        public void SynchronousRequestReply()
        {
            var responder = new PoolFiber();
            responder.Start();
            var timeCheck = new RequestReplyChannel<string, DateTime>();
            var now = DateTime.Now;
            Action<IRequest<string, DateTime>> onRequest = req => req.SendReply(now);
            timeCheck.Subscribe(responder, onRequest);
            var response = timeCheck.SendRequest("hello");
            DateTime result;
            Assert.IsTrue(response.Receive(10000, out result));
            Assert.AreEqual(result, now);
        }

        [Test]
        public void SynchronousRequestWithMultipleReplies()
        {
            IFiber responder = new PoolFiber();
            responder.Start();
            var countChannel = new RequestReplyChannel<string, int>();

            var allSent = new AutoResetEvent(false);
            Action<IRequest<string, int>> onRequest =
                delegate(IRequest<string, int> req)
                {
                    for (var i = 0; i <= 5; i++)
                        req.SendReply(i);
                    allSent.Set();
                };
            countChannel.Subscribe(responder, onRequest);
            var response = countChannel.SendRequest("hello");
            int result;
            using (response)
            {
                for (var i = 0; i < 5; i++)
                {
                    Assert.IsTrue(response.Receive(10000, out result));
                    Assert.AreEqual(result, i);
                }
                allSent.WaitOne(10000, false);
            }
            Assert.IsTrue(response.Receive(30000, out result));
            Assert.AreEqual(5, result);
            Assert.IsFalse(response.Receive(30000, out result));
        }
    }
}