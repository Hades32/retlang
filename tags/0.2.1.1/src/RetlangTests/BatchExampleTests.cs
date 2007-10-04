using System;
using System.Collections.Generic;
using NUnit.Framework;
using Retlang;

namespace RetlangTests
{
    [TestFixture]
    public class BatchExampleTests
    {
        [Test]
        public void MessagesInFiftyMilliseconds()
        {
            ProcessContextFactory factory = ProcessFactoryFixture.CreateAndStart();

            IProcessContext context = factory.CreateAndStart();

            IsMatch<object> selector = delegate { return true; };
            TopicSelector<object> topicMatcher = new TopicSelector<object>(selector);
            On<IList<IMessageEnvelope<object>>> messageCount = delegate(IList<IMessageEnvelope<object>> msgs)
                                                                   {
                                                                       Console.WriteLine("Message Count: " + msgs.Count);
                                                                       context.Stop();
                                                                   };
            context.SubscribeToBatch<object>(topicMatcher, messageCount, 50);

            for (int i = 0; i < 100; i++)
            {
                context.Publish(new object(), new object());
                context.Publish("string.topic", i);
                Command command = delegate { };
                context.Publish("command.topic", command);
            }

            context.Join();
            factory.Stop();
            factory.Join();
        }

        [Test]
        public void UniqueMessagesByTopicInFiftyMilliseconds()
        {
            ProcessContextFactory factory = ProcessFactoryFixture.CreateAndStart();

            IProcessContext context = factory.CreateAndStart();

            IsMatch<object> selector = delegate { return true; };
            TopicSelector<object> topicMatcher = new TopicSelector<object>(selector);
            On<IDictionary<object, IMessageEnvelope<object>>> messageCount =
                delegate(IDictionary<object, IMessageEnvelope<object>> msgs)
                    {
                        Console.WriteLine("Message Count: " + msgs.Values.Count);
                        context.Stop();
                    };
            ResolveKey<object, object> keyResolver =
                delegate(IMessageHeader header, object msg) { return header.Topic; };
            context.SubscribeToKeyedBatch<object, object>(topicMatcher, keyResolver, messageCount, 50);

            for (int i = 0; i < 100; i++)
            {
                context.Publish(new object(), new object());
                context.Publish("string.topic", i);
                Command command = delegate { };
                context.Publish("command.topic", command);
            }

            context.Join();
            factory.Stop();
            factory.Join();
        }
    }
}