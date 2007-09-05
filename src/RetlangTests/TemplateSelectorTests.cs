using System;
using System.Collections.Generic;
using NUnit.Framework;
using Retlang;
using Rhino.Mocks;

namespace RetlangTests
{
    [TestFixture]
    public class TemplateSelectorTests
    {
        [Test]
        public void Select()
        {
            MockRepository repo = new MockRepository();
            IsMatch<string> matcher = repo.CreateMock<IsMatch<string>>(); 
            
            Expect.Call(matcher("one")).Return(true);
            Expect.Call(matcher("other")).Return(false);

            repo.ReplayAll();

            TemplateSelector<string> selector = new TemplateSelector<string>(matcher);
            Assert.IsTrue(selector.Matches("one"));
            Assert.IsFalse(selector.Matches("other"));
            Assert.IsFalse(selector.Matches(1));
        }
    }
}
