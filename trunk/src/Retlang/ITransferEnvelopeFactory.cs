namespace Retlang
{
    /// <summary>
    /// Wraps a message for transfer across threads.
    /// </summary>
    public interface ITransferEnvelopeFactory
    {
        /// <summary>
        /// Creates an envelope.
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="msg"></param>
        /// <param name="replyTo"></param>
        /// <returns></returns>
        ITransferEnvelope Create(object topic, object msg, object replyTo);
    }

    /// <summary>
    /// Create a simple wrapper for the object. Does not serialize or copy the object.  
    /// </summary>
    public class ObjectTransferEnvelopeFactory : ITransferEnvelopeFactory
    {

        /// <summary>
        /// <see cref="ITransferEnvelopeFactory.Create(object,object,object)"/>
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="msg"></param>
        /// <param name="replyTo"></param>
        /// <returns></returns>
        public ITransferEnvelope Create(object topic, object msg, object replyTo)
        {
            return new ObjectTransferEnvelope(msg, new MessageHeader(topic, replyTo));
        }
    }

    /// <summary>
    /// Serializes the object and creates a copy for each thread when the message is delivered.
    /// </summary>
    public class BinaryTransferEnvelopeFactory : ITransferEnvelopeFactory
    {
        /// <summary>
        /// <see cref="ITransferEnvelopeFactory.Create(object,object,object)"/>
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="msg"></param>
        /// <param name="replyTo"></param>
        /// <returns></returns>
        public ITransferEnvelope Create(object topic, object msg, object replyTo)
        {
            return new BinaryTransferEnvelope(topic, msg, replyTo);
        }
    }

    /// <summary>
    /// Uses the XmlSerializer to create a defensive clone.
    /// </summary>
    public class XmlTransferEnvelopeFactory : ITransferEnvelopeFactory
    {
        /// <summary>
        /// <see cref="ITransferEnvelopeFactory.Create(object,object,object)"/>
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="msg"></param>
        /// <param name="replyTo"></param>
        /// <returns></returns>
        public ITransferEnvelope Create(object topic, object msg, object replyTo)
        {
            return new XmlTransferEnvelope(topic, msg, replyTo);
        }
    }
}