namespace Retlang
{
    /// <summary>
    /// Matches a topic.
    /// </summary>
    public interface ITopicMatcher
    {
        /// <summary>
        /// Return true on match.
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        bool Matches(object topic);
    }
}