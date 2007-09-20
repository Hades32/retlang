using System;

namespace Retlang
{
    public interface ITransferEnvelope
    {
        Type MessageType { get; }
        IMessageHeader Header { get; }

        /// <summary>
        /// Called when the message is delivered to an individual subscriber.
        /// </summary>
        /// <returns></returns>
        object ResolveMessage();
    }
}