using System.Text.RegularExpressions;

namespace Retlang
{
    public class TopicRegex : ITopicMatcher
    {
        private readonly Regex _regex;

        public TopicRegex(Regex reg)
        {
            _regex = reg;
        }

        public bool Matches(object topic)
        {
            string topicStr = topic as string;
            if (topicStr != null)
            {
                return _regex.IsMatch(topicStr);
            }
            return false;
        }
    }
}