using System;
using System.Collections.Generic;
using System.Text;

namespace Retlang
{
    public class TopicEquals : TopicSelector<object>
    {
        private readonly object _toMatch;

        public TopicEquals(object toMatch)
            :base(toMatch.Equals)
        {
            _toMatch = toMatch;
        }

    }
}
