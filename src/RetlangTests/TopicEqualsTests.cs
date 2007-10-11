using System;
using System.Collections.Generic;
using Retlang;
using NUnit.Framework;

namespace RetlangTests
{
    [TestFixture]
    public class TopicEqualsTests
    {
        [Test]
        public void Equality()
        {
            TopicEquals topic = new TopicEquals("stuff");
            Assert.AreEqual(topic, new TopicEquals("stuf"+"f"));
            Assert.AreEqual(topic.GetHashCode(), new TopicEquals("stuff").GetHashCode());
            Assert.AreNotEqual(topic, new TopicEquals("other"));
            Assert.AreNotEqual(topic, new TopicSelector<string>(delegate{return false;}));
        }
    }
}
