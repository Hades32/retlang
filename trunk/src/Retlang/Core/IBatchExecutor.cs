using System;

namespace Retlang.Core
{
    /// <summary>
    /// Executes the pending events on for the process bus.
    /// </summary>
    public interface IBatchExecutor
    {
        /// <summary>
        /// Execute all pending events for the process bus.
        /// </summary>
        /// <param name="toExecute"></param>
        void ExecuteAll(Action[] toExecute);
    }
}