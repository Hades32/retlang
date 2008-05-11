namespace Retlang
{
    /// <summary>
    /// Return true if the topic matches.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="topic"></param>
    /// <returns></returns>
    public delegate bool IsMatch<T>(T topic);


    /// <summary>
    /// Matches the topic based upon the generic type and provided matcher.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TopicSelector<T> : ITopicMatcher
    {
        private readonly IsMatch<T> _match;

        /// <summary>
        /// Construct selector with provided matcher.
        /// </summary>
        /// <param name="matcher"></param>
        public TopicSelector(IsMatch<T> matcher)
        {
            _match = matcher;
        }

        /// <summary>
        /// Returns true if the topic type matches the generic type
        /// and the matcher returns true.
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public bool Matches(object topic)
        {
            if (topic is T)
            {
                T typedObject = (T) topic;
                return _match(typedObject);
            }
            return false;
        }
    }
}