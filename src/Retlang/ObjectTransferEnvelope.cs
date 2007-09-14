using System;
using System.Collections.Generic;
using System.Text;

namespace Retlang
{
    public class ObjectTransferEnvelope: ITransferEnvelope
    {
        private readonly IMessageHeader _header;
        private readonly object _obj;

        public ObjectTransferEnvelope(object obj, IMessageHeader header)
        {
            if (obj == null)
            {
                throw new NullReferenceException("Message Cannot be null");
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
