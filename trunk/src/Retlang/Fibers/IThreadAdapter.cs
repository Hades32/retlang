using System;

namespace Retlang.Fibers
{
    /// <summary>
    /// Invokes action on another thread
    /// </summary>
    public interface IThreadAdapter
    {
        /// <summary>
        /// Invokes action on another thread
        /// </summary>
        /// <param name="action"></param>
        void Invoke(Action action);
    }
}