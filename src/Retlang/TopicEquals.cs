namespace Retlang
{
    /// <summary>
    /// Matches topic using the object equals methods.
    /// <seealso cref="object.Equals(object)"/>
    /// </summary>
    public class TopicEquals : ITopicMatcher
    {
        private readonly object _toMatch;

        /// <summary>
        /// Construct new matcher.
        /// </summary>
        /// <param name="toMatch"></param>
        public TopicEquals(object toMatch)
        {
            _toMatch = toMatch;
        }

        /// <summary>
        /// Returns the hashcode of the matching object.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            if (_toMatch == null)
            {
                return 0;
            }
            return _toMatch.GetHashCode();
        }

        /// <summary>
        /// <see cref="object.Equals(object)"/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            TopicEquals otherEquals = obj as TopicEquals;
            if (otherEquals == null)
            {
                return false;
            }
            return _toMatch == otherEquals._toMatch;
        }

        /// <summary>
        /// <see cref="ITopicMatcher.Matches(object)"/>
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public bool Matches(object topic)
        {
            return _toMatch.Equals(topic);
        }

        /// <summary>
        /// <see cref="object.ToString()"/>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "TopicEquals:" + _toMatch;
        }
    }
}