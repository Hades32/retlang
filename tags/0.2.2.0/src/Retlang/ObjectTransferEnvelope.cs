using System;

namespace Retlang
{
    /// <summary>
    /// Transfer objects between contexts without serialization.
    /// </summary>
    public class ObjectTransferEnvelope : ITransferEnvelope
    {
        private readonly IMessageHeader _header;
        private readonly object _obj;

        /// <summary>
        /// </summary>
        /// <param name="obj">data to publish</param>
        /// <param name="header">header</param>
        public ObjectTransferEnvelope(object obj, IMessageHeader header)
        {
            if (obj == null)
            {
                throw new NullReferenceException("Message cannot be null");
            }
            _obj = obj;
            _header = header;
        }

        public Type MessageType
        {
            get { return _obj.GetType(); }
        }

        public object ResolveMessage()
        {
            return _obj;
        }

        public IMessageHeader Header
        {
            get { return _header; }
        }
    }
}