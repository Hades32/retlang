using System;

namespace Retlang
{
    /// <summary>
    /// Packages a message for transfer between contexts.
    /// </summary>
    public interface ITransferEnvelope
    {
        /// <summary>
        /// The type of the message contained in the envelope.
        /// </summary>
        Type MessageType { get; }

        /// <summary>
        /// The topic and reply topic of the message.
        /// </summary>
        IMessageHeader Header { get; }

        /// <summary>
        /// Called when the message is delivered to an individual subscriber.
        /// </summary>
        /// <returns></returns>
        object ResolveMessage();

        /// <summary>
        /// Determines whether the message can be cast to the generic type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool CanCastTo<T>();
    }
}