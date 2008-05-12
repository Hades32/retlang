using Retlang;
using NUnit.Framework;
using System;

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
                    req.SendResponse(now);
                };
                timeCheck.Subscribe(responder, onRequest);
                IChannelResponse<DateTime> response = timeCheck.SendRequest("hello");
                DateTime result;
                Assert.IsTrue(response.Receive(10000, out result));
                Assert.AreEqual(result, now);

            }
        }
    }
}
