using System.Text.RegularExpressions;

namespace Retlang
{
    /// <summary>
    /// Matches topic based upon a regular expression. Topic must be a string.
    /// </summary>
    public class TopicRegex : ITopicMatcher
    {
        private readonly Regex _regex;

        /// <summary>
        /// Create a new instance with the provided Regex.
        /// </summary>
        /// <param name="reg"></param>
        public TopicRegex(Regex reg)
        {
            _regex = reg;
        }

        /// <summary>
        /// Matches if the topic is a string and it matches the Regex.
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
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