using Retlang.Core;

namespace Retlang.Fibers
{
    /// <summary>
    /// Factory for creating backing threads.
    /// </summary>
    public interface IThreadFiberFactory
    {
        /// <summary>
        /// Create ThreadFiber with IBatchExecutor
        /// </summary>
        /// <param name="executor"></param>
        /// <returns></returns>
        IThreadFiber CreateThreadFiber(IBatchExecutor executor);

        /// <summary>
        /// Create named fiber thread.
        /// </summary>
        /// <param name="executor"></param>
        /// <param name="threadName"></param>
        /// <returns></returns>
        IThreadFiber CreateThreadFiber(IBatchExecutor executor, string threadName);
    }
}
