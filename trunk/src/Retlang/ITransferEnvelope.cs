using System;

namespace Retlang
{
    public interface ITransferEnvelope
    {
        Type MessageType { get; }
        object ResolveMessage { get; }
        IMessageHeader Header { get; }
    }
}
