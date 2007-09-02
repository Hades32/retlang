using System;
using System.Collections.Generic;
using System.Text;

namespace Retlang
{
    public delegate bool IsMatch(object topic);

    public class TopicSelector: ITopicMatcher
    {
        private readonly IsMatch _selector;

        public TopicSelector(IsMatch selector)
        {
            _selector = selector;
        }

        public bool Matches(object topic)
        {
            return _selector(topic);
        }
    }
}
