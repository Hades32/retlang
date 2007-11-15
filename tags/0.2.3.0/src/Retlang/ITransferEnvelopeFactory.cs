namespace Retlang
{
    public interface ITransferEnvelopeFactory
    {
        ITransferEnvelope Create(object topic, object msg, object replyTo);
    }

    public class ObjectTransferEnvelopeFactory : ITransferEnvelopeFactory
    {
        public ITransferEnvelope Create(object topic, object msg, object replyTo)
        {
            return new ObjectTransferEnvelope(msg, new MessageHeader(topic, replyTo));
        }
    }

    public class BinaryTransferEnvelopeFactory : ITransferEnvelopeFactory
    {
        public ITransferEnvelope Create(object topic, object msg, object replyTo)
        {
            return new BinaryTransferEnvelope(topic, msg, replyTo);
        }
    }

    public class XmlTransferEnvelopeFactory : ITransferEnvelopeFactory
    {
        public ITransferEnvelope Create(object topic, object msg, object replyTo)
        {
            return new XmlTransferEnvelope(topic, msg, replyTo);
        }
    }
}