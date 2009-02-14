using System;

namespace Retlang.Core
{
    ///<summary>
    /// Executes pending action(s).
    ///</summary>
    public interface IBatchAndSingleExecutor : IBatchExecutor
    {
        ///<summary>
        /// Execute a single pending action.
        ///</summary>
        ///<param name="action"></param>
        void Execute(Action action);
    }
}