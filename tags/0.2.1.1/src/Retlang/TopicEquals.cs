namespace Retlang
{
    public class TopicEquals : TopicSelector<object>
    {
        private readonly object _toMatch;

        public TopicEquals(object toMatch)
            : base(toMatch.Equals)
        {
            _toMatch = toMatch;
        }

        public override int GetHashCode()
        {
            if(_toMatch == null)
            {
                return 0;
            }
            return _toMatch.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            TopicEquals otherEquals = obj as TopicEquals;
            if(otherEquals == null)
            {
                return false;
            }
            return _toMatch == otherEquals._toMatch;
        }
    }
}