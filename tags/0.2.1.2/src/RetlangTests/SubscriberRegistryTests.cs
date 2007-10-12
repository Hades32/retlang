
using NUnit.Framework;
using Retlang;
using Rhino.Mocks;

namespace RetlangTests
{
    [TestFixture]
    public class SubscriberRegistryTests
    {

        [Test]
        public void Subscribe()
        {
            MockRepository mocks = new MockRepository();
            OnMessage<string> callback = mocks.CreateMock<OnMessage<string>>();
            callback(null, null);
            LastCall.IgnoreArguments();
            OnMessage<string> otherCallback = mocks.CreateMock<OnMessage<string>>();
            otherCallback(null, null);
            LastCall.IgnoreArguments().Repeat.Twice();
            mocks.ReplayAll();

            SubscriberRegistry registry = new SubscriberRegistry();
            TopicEquals topic = new TopicEquals("topic");
            
            TopicSubscriber<string> sub = new TopicSubscriber<string>(topic, callback);
            registry.Subscribe(sub);

            TopicSubscriber<string> other = new TopicSubscriber<string>(topic, otherCallback);
            registry.Subscribe(other);

            registry.Publish(new ObjectTransferEnvelope("data", new MessageHeader("topic", null)));

            registry.Unsubscribe(sub);

            registry.Publish(new ObjectTransferEnvelope("data", new MessageHeader("topic", null)));

            mocks.VerifyAll();
        }
    }
}
