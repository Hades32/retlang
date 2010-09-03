using System;

namespace Retlang.Core
{
    /// <summary>
    /// Enqueues actions.
    /// </summary>
    public interface IContext
    {
        /// <summary>
        /// Enqueue a single action.
        /// </summary>
        /// <param name="action"></param>
        void Enqueue(Action action);
    }
}