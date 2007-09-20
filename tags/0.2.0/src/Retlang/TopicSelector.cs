using System;

namespace Retlang
{
    public delegate bool IsMatch<T>(T topic);

    public class TopicSelector<T> : ITopicMatcher
    {
        private IsMatch<T> _match;

        public TopicSelector(IsMatch<T> matcher)
        {
            _match = matcher;
        }

        private Type FilterType
        {
            get { return typeof (T); }
        }

        public bool Matches(object topic)
        {
            if (FilterType.IsAssignableFrom(topic.GetType()))
            {
                T typedObject = (T) topic;
                return _match(typedObject);
            }
            return false;
        }
    }
}