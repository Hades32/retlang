using Retlang.Fibers;
using Windows.System.Threading;

namespace WPTest
{
    public class ThreadPoolThread : IThread
    {
        private readonly System.Action action;
        private Windows.Foundation.IAsyncAction task;

        public ThreadPoolThread(System.Action action)
        {
            this.action = action;
        }

        public void Start()
        {
            task = ThreadPool.RunAsync(OnAction);
        }

        private void OnAction(Windows.Foundation.IAsyncAction operation)
        {
            action();
        }

        public void Join()
        {
            if (task == null)
                return;

            task.GetResults();
            task.Close();
        }
    }
}
