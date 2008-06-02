namespace Retlang
{
    /// <summary>
    /// The topic and reply topic for the message.
    /// </summary>
    public interface IMessageHeader
    {
        /// <summary>
        /// The published topic
        /// </summary>
        object Topic { get; }

        /// <summary>
        /// optional reply topic
        /// </summary>
        object ReplyTo { get; }
    }

    /// <summary>
    /// Default MesageHeader implementation.
    /// </summary>
    public class MessageHeader : IMessageHeader
    {
        private readonly object _topic;
        private readonly object _replyTo;

        /// <summary>
        /// Construct Header.
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="replyTo"></param>
        public MessageHeader(object topic, object replyTo)
        {
            _topic = topic;
            _replyTo = replyTo;
        }

        /// <summary>
        /// <see cref="IMessageHeader.Topic"/>
        /// </summary>
        public object Topic
        {
            get { return _topic; }
        }

        /// <summary>
        /// <see cref="IMessageHeader.ReplyTo"/>
        /// </summary>
        public object ReplyTo
        {
            get { return _replyTo; }
        }

        /// <summary>
        /// <see cref="object.GetHashCode()"/>
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Topic.GetHashCode();
        }

        /// <summary>
        /// Compares topic and replyTo properties.
        /// <see cref="object.Equals(object)"/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
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