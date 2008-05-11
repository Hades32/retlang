using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Retlang
{
    /// <summary>
    /// Transfer Envelope that uses object serialization to create defensive copies when passing objects across thread.
    /// </summary>
    internal class BinaryTransferEnvelope : ITransferEnvelope
    {
        private readonly Type _messageType;
        private readonly byte[] _msg;
        private readonly IMessageHeader _header;

        /// <summary>
        /// Construct a new instance. Message is serialized to bytes during construction.
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="msg"></param>
        /// <param name="replyTo"></param>
        public BinaryTransferEnvelope(object topic, object msg, object replyTo)
        {
            if (msg == null)
            {
                throw new NullReferenceException("Message cannot be null");
            }
            _messageType = msg.GetType();
            _header = new MessageHeader(topic, replyTo);
            _msg = ConvertToBytes(msg);
        }

        internal byte[] ConvertToBytes(object obj)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, obj);
                stream.Flush();
                return stream.ToArray();
            }
        }

        public Type MessageType
        {
            get { return _messageType; }
        }

        public bool CanCastTo<T>()
        {
            return typeof (T).IsAssignableFrom(MessageType);
        }

        public object ResolveMessage()
        {
            using (MemoryStream stream = new MemoryStream(_msg))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return formatter.Deserialize(stream);
            }
        }

        public IMessageHeader Header
        {
            get { return _header; }
        }
    }
}