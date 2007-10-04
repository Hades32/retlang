namespace Retlang
{
    public interface IMessageEnvelope<T>
    {
        IMessageHeader Header { get; }
        T Message { get; }
    }

    public class MessageEnvelope<T> : IMessageEnvelope<T>
    {
        private readonly IMessageHeader _header;
        private readonly T _msg;

        public MessageEnvelope(IMessageHeader header, T msg)
        {
            _header = header;
            _msg = msg;
        }

        public IMessageHeader Header
        {
            get { return _header; }
        }

        public T Message
        {
            get { return _msg; }
        }
    }
}