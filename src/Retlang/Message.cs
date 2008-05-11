namespace Retlang
{
    /// <summary>
    /// Wraps the message body and header.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMessageEnvelope<T>
    {
        /// <summary>
        /// Topic and reply to
        /// </summary>
        IMessageHeader Header { get; }
        /// <summary>
        /// Body
        /// </summary>
        T Message { get; }
    }

    /// <summary>
    /// Default implementation for IMessageEnvelope
    /// <see cref="IMessageEnvelope{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MessageEnvelope<T> : IMessageEnvelope<T>
    {
        private readonly IMessageHeader _header;
        private readonly T _msg;

        /// <summary>
        /// new instance.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="msg"></param>
        public MessageEnvelope(IMessageHeader header, T msg)
        {
            _header = header;
            _msg = msg;
        }
        /// <summary>
        /// <see cref="IMessageEnvelope{T}.Header"/>
        /// </summary>
        public IMessageHeader Header
        {
            get { return _header; }
        }

        /// <summary>
        /// <see cref="IMessageEnvelope{T}.Message"/>
        /// </summary>
        public T Message
        {
            get { return _msg; }
        }
    }
}