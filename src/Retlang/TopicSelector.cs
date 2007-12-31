namespace Retlang
{
    public delegate bool IsMatch<T>(T topic);

    public class TopicSelector<T> : ITopicMatcher
    {
        private readonly IsMatch<T> _match;

        public TopicSelector(IsMatch<T> matcher)
        {
            _match = matcher;
        }

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