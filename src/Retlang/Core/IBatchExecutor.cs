using System;

namespace Retlang.Core
{
    /// <summary>
    /// Executes pending action(s).
    /// </summary>
    public interface IBatchExecutor
    {
        /// <summary>
        /// Execute all pending actions.
        /// </summary>
        /// <param name="toExecute"></param>
        void ExecuteAll(Action[] toExecute);
    }
}