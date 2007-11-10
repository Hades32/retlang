namespace Retlang
{
    public interface IMessageHeader
    {
        object Topic { get; }
        object ReplyTo { get; }
    }

    public class MessageHeader : IMessageHeader
    {
        private readonly object _topic;
        private readonly object _replyTo;

        public MessageHeader(object topic, object replyTo)
        {
            _topic = topic;
            _replyTo = replyTo;
        }

        public object Topic
        {
            get { return _topic; }
        }

        public object ReplyTo
        {
            get { return _replyTo; }
        }

        public override int GetHashCode()
        {
            return Topic.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            IMessageHeader header = obj as IMessageHeader;
            if (header == null)
            {
                return false;
            }
            return Topic == header.Topic && ReplyTo == header.ReplyTo;
        }
    }
}