using System;
using System.Collections.Generic;
using System.Text;

namespace Retlang
{
    public interface ITopicMatcher
    {
        bool Matches(object topic);
    }

    public class TopicMatcher : ITopicMatcher
    {
        private readonly object _toMatch;

        public TopicMatcher(object toMatch)
        {
            _toMatch = toMatch;
        }

        public bool Matches(object topic)
        {
            return _toMatch.Equals(topic);
        }
    }
}
