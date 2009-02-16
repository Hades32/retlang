using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.ComponentModel;
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

    class DispatcherAdapter : IThreadAdapter
    {
        private readonly Dispatcher dispatcher;

        public DispatcherAdapter(Dispatcher d)
        {
            this.dispatcher = d;
        }

        public void Invoke(Action method)
        {
            dispatcher.BeginInvoke(method);
        }
    }
}
