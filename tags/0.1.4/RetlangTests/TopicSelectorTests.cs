using System;
using System.Collections.Generic;
using NUnit.Framework;
using Retlang;
using Rhino.Mocks;

namespace RetlangTests
{
    [TestFixture]
    public class TopicSelectorTests
    {
        [Test]
        public void Select()
        {
            MockRepository repo = new MockRepository();
            IsMatch<string> matcher = repo.CreateMock<IsMatch<string>>(); 
            
            Expect.Call(matcher("one")).Return(true);
            Expect.Call(matcher("other")).Return(false);

            repo.ReplayAll();

            TopicSelector<string> selector = new TopicSelector<string>(matcher);
            Assert.IsTrue(selector.Matches("one"));
            Assert.IsFalse(selector.Matches("other"));
            Assert.IsFalse(selector.Matches(1));
        }
    }
}
