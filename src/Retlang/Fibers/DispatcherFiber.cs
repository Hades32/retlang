using System;
using System.Windows.Threading;
using Retlang.Core;

namespace Retlang.Fibers
{
    /// <summary>
    /// Adapts Dispatcher to a Fiber
    /// </summary>
    public class DispatcherFiber : BaseFiber
    {
        /// <summary>
        /// Constructs a Fiber that executes on dispatcher thread
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <param name="executor"></param>
        public DispatcherFiber(Dispatcher dispatcher, IBatchAndSingleExecutor executor)
            : base(new DispatcherAdapter(dispatcher), executor)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dispatcher"></param>
        public DispatcherFiber(Dispatcher dispatcher)
            : this(dispatcher, new BatchAndSingleExecutor())
        {
        }
    }

    internal class DispatcherAdapter : IThreadAdapter
    {
        private readonly Dispatcher _dispatcher;

        public DispatcherAdapter(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public void Invoke(Action method)
        {
            _dispatcher.BeginInvoke(method);
        }
    }
}