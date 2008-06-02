using Retlang;
using NUnit.Framework;
using System;
using System.Threading;

namespace RetlangTests
{
    [TestFixture]
    public class RequestReplyChannelTests
    {
        [Test]
        public void SynchronousRequestReply()
        {
            using (ProcessContextFactory fact = ProcessFactoryFixture.CreateAndStart())
            {
                IProcessBus responder = fact.CreatePooledAndStart();
                RequestReplyChannel<string, DateTime> timeCheck = new RequestReplyChannel<string, DateTime>();
                DateTime now = DateTime.Now;
                Action<IChannelRequest<string,DateTime>> onRequest = 
                delegate(IChannelRequest<string,DateTime> req){
                    req.SendReply(now);
                };
                timeCheck.Subscribe(responder, onRequest);
                IChannelReply<DateTime> response = timeCheck.SendRequest("hello");
                DateTime result;
                Assert.IsTrue(response.Receive(10000, out result));
                Assert.AreEqual(result, now);

            }
        }


        [Test]
        public void SynchronousRequestWithMultipleReplies()
        {
            using (ProcessContextFactory fact = ProcessFactoryFixture.CreateAndStart())
            {
                IProcessBus responder = fact.CreatePooledAndStart();
                RequestReplyChannel<string, int> countChannel = new RequestReplyChannel<string, int>();

                AutoResetEvent allSent = new AutoResetEvent(false);
                Action<IChannelRequest<string, int>> onRequest =
                delegate(IChannelRequest<string, int> req)
                {
                    for (int i = 0; i <= 5; i++ )
                        req.SendReply(i);
                    allSent.Set();
                };
                countChannel.Subscribe(responder, onRequest);
                IChannelReply<int> response = countChannel.SendRequest("hello");
                int result;
                using (response)
                {
                    for (int i = 0; i < 5; i++)
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
}
