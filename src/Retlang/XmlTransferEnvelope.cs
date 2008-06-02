using System;
using System.IO;
using System.Xml.Serialization;

namespace Retlang
{
    /// <summary>
    /// Uses xml serialization for defensive cloning.
    /// </summary>
    public class XmlTransferEnvelope : ITransferEnvelope
    {
        private readonly Type _messageType;
        private readonly byte[] _msg;
        private readonly IMessageHeader _header;

        /// <summary>
        /// Creates a new instance and copies the message using xml serialization.
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="msg"></param>
        /// <param name="replyTo"></param>
        public XmlTransferEnvelope(object topic, object msg, object replyTo)
        {
            if (msg == null)
            {
                throw new NullReferenceException("Message cannot be null");
            }
            _messageType = msg.GetType();
            _header = new MessageHeader(topic, replyTo);
            _msg = ConvertToBytes(msg, _messageType);
        }

        /// <summary>
        /// <see cref="ITransferEnvelope.CanCastTo{T}()"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool CanCastTo<T>()
        {
            return typeof (T).IsAssignableFrom(MessageType);
        }

        private byte[] ConvertToBytes(object obj, Type typ)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                XmlSerializer formatter = new XmlSerializer(typ);
                formatter.Serialize(stream, obj);
                stream.Flush();
                return stream.ToArray();
            }
        }

        /// <summary>
        /// <see cref="ITransferEnvelope.MessageType"/>
        /// </summary>
        public Type MessageType
        {
            get { return _messageType; }
        }

        /// <summary>
        /// Parses the xml bytes into a new object.
        /// </summary>
        /// <returns></returns>
        public object ResolveMessage()
        {
            using (MemoryStream stream = new MemoryStream(_msg))
            {
                XmlSerializer formatter = new XmlSerializer(_messageType);
                return formatter.Deserialize(stream);
            }
        }

        /// <summary>
        /// <see cref="ITransferEnvelope.Header"/>
        /// </summary>
        public IMessageHeader Header
        {
            get { return _header; }
        }
    }
}