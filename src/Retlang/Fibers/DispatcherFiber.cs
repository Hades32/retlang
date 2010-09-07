using System.Windows.Threading;
using Retlang.Core;

namespace Retlang.Fibers
{
    /// <summary>
    /// Adapts Dispatcher to a Fiber. Transparently moves actions onto the Dispatcher thread.
    /// </summary>
    public class DispatcherFiber : GuiFiber
    {
        /// <summary>
        /// Constructs a Fiber that executes on dispatcher thread.
        /// </summary>
        /// <param name="dispatcher">The dispatcher.</param>
        /// <param name="priority">The priority.</param>
        /// <param name="executor">The executor.</param>
        public DispatcherFiber(Dispatcher dispatcher, DispatcherPriority priority, IExecutor executor)
            : base(new DispatcherAdapter(dispatcher, priority), executor)
        {
        }

        /// <summary>
        /// Constructs a Fiber that executes on dispatcher thread.
        /// </summary>
        /// <param name="dispatcher">The dispatcher.</param>
        /// <param name="executor">The priority.</param>
        public DispatcherFiber(Dispatcher dispatcher, IExecutor executor)
            : this(dispatcher, DispatcherPriority.Normal, executor)
        {
        }

        /// <summary>
        /// Constructs a Fiber that executes on dispatcher thread.
        /// </summary>
        /// <param name="dispatcher">The dispatcher.</param>
        /// <param name="priority">The priority.</param>
        public DispatcherFiber(Dispatcher dispatcher, DispatcherPriority priority)
            : this(dispatcher, priority, new DefaultExecutor())
        {
        }

        /// <summary>
        /// Constructs a Fiber that executes on dispatcher thread.
        /// </summary>
        /// <param name="dispatcher">The dispatcher.</param>
        public DispatcherFiber(Dispatcher dispatcher)
            : this(dispatcher, new DefaultExecutor())
        {
        }

        /// <summary>
        /// Constructs a Fiber that executes on dispatcher thread of the
        /// current dispatcher.
        /// </summary>
        /// <param name="priority">The priority.</param>
        public DispatcherFiber(DispatcherPriority priority)
            : this(Dispatcher.CurrentDispatcher, priority, new DefaultExecutor())
        {
        }

        /// <summary>
        /// Constructs a Fiber that executes on dispatcher thread of the
        /// current dispatcher.
        /// </summary>
        public DispatcherFiber()
            : this(Dispatcher.CurrentDispatcher, new DefaultExecutor())
        {
        }
    }
}