
namespace Retlang.Core
{
    ///// <summary>
    ///// Default implementation that uses the .NET thread pool.
    ///// </summary>
    //public class DefaultThreadPool : IThreadPool
    //{
    //    /// <summary>
    //    /// Enqueues action.
    //    /// </summary>
    //    /// <param name="callback"></param>
    //    public void Queue(WaitCallback callback)
    //    {
    //        if (!ThreadPool.QueueUserWorkItem(callback))
    //        {
    //            throw new QueueFullException("Unable to add item to pool: " + callback.Target);
    //        }
    //    }
    //}
}