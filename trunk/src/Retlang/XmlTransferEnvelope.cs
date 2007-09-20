using System;
using System.IO;
using System.Xml.Serialization;

namespace Retlang
{
    public class XmlTransferEnvelope : ITransferEnvelope
    {
        private readonly Type _messageType;
        private readonly byte[] _msg;
        private readonly IMessageHeader _header;

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

        public Type MessageType
        {
            get { return _messageType; }
        }

        public object ResolveMessage()
        {
            using (MemoryStream stream = new MemoryStream(_msg))
            {
                XmlSerializer formatter = new XmlSerializer(_messageType);
                return formatter.Deserialize(stream);
            }
        }

        public IMessageHeader Header
        {
            get { return _header; }
        }
    }
}