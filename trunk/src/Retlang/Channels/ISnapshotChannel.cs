using System;
using Retlang.Fibers;

namespace Retlang.Channels
{
    ///<summary>
    /// An ISnapshotChannel is a channel that allows for the transmission of an initial snapshot followed by incremental updates.
    /// The class is thread safe.
    ///</summary>
    ///<typeparam name="T"></typeparam>
    public interface ISnapshotChannel<T>
    {
        ///<summary>
        /// Subscribes for an initial snapshot and then incremental update.
        ///</summary>
        ///<param name="fiber">the target executor to receive the message</param>
        ///<param name="receive"></param>
        void PrimedSubscribe(IFiber fiber, Action<T> receive);
        ///<summary>
        /// Publishes the incremental update.
        ///</summary>
        ///<param name="update"></param>
        void Publish(T update);
        ///<summary>
        /// Ressponds to the request for an initial snapshot.
        ///</summary>
        ///<param name="fiber">the target executor to receive the message</param>
        ///<param name="reply">returns the snapshot update</param>
        void ReplyToPrimingRequest(IFiber fiber, Func<T> reply);
    }
}