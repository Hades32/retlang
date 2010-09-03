using System;
using System.Collections.Generic;

namespace Retlang.Core
{
    /// <summary>
    /// Executes pending action(s).
    /// </summary>
    public interface IExecutor
    {
        /// <summary>
        /// Execute all pending actions.
        /// </summary>
        /// <param name="toExecute"></param>
        void ExecuteAll(List<Action> toExecute);
    }
}