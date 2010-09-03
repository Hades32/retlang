using System.Threading;

namespace Retlang.Core
{
    /// <summary>
    /// A thread pool for executing asynchronous actions.
    /// </summary>
    public interface IThreadPool
    {
        /// <summary>
        /// Enqueue action for execution.
        /// </summary>
        /// <param name="callback"></param>
        void Queue(WaitCallback callback);
    }
}