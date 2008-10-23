using System.Text.RegularExpressions;
using NUnit.Framework;
using Retlang;

namespace RetlangTests
{
    [TestFixture]
    public class TopicRegexTests
    {
        [Test]
        public void TestRegex()
        {
            Regex reg = new Regex("^a");
            TopicRegex match = new TopicRegex(reg);
            Assert.IsTrue(match.Matches("abc"));
            Assert.IsFalse(match.Matches("cde"));
            Assert.IsFalse(match.Matches(new object()));
        }
    }
}