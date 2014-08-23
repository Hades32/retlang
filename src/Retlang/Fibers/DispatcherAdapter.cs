using Retlang.Core;
using System;
using System.Threading;

namespace Retlang.Fibers
{
    internal class DispatcherAdapter : IExecutionContext
    {
        private readonly SynchronizationContext _dispatcher;

        public DispatcherAdapter(SynchronizationContext dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public void Enqueue(Action action)
        {
            _dispatcher.Post((_) => action(), null);
        }
    }
}