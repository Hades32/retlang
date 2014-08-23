using System.Threading.Tasks;

namespace Retlang.Fibers
{
    public class TaskThread : IThread
    {
        private readonly System.Action action;
        private Task task;

        public TaskThread(System.Action action)
        {
            this.action = action;
        }

        public void Start()
        {
            task = Task.Run(action);
        }

        public void Join()
        {
            if (task == null)
                return;

            task.Wait();
        }
    }
}
