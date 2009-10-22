using System;
using Retlang.Core;

namespace Retlang.Channels
{
    public interface ISnapshotChannel<T>
    {
        void PrimedSubscribe(IDisposingExecutor fiber, Action<T> handler);
        void Publish(T update);
        void ReplyToPrimingRequest(IDisposingExecutor fiber, Func<T> getter);
    }
}