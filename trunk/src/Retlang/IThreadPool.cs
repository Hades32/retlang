using System.Threading;

namespace Retlang
{
    public interface IThreadPool
    {
        void Queue(WaitCallback callback);
    }

    public class DefaultThreadPool : IThreadPool
    {
        public void Queue(WaitCallback callback)
        {
            if (!ThreadPool.QueueUserWorkItem(callback))
            {
                throw new QueueFullException("Unable to add item to pool: " + callback.Target);
            }
        }
    }
}
