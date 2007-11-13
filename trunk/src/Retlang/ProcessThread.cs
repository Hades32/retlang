using System.Threading;

namespace Retlang
{
    public interface IProcessQueue : ICommandQueue, ICommandTimer, IThreadController
    {
    }

    public interface IProcessThread : IProcessQueue
    {
        Thread Thread { get; }
    }

    public class ProcessThread : IProcessThread
    {
        private static int THREAD_COUNT = 0;

        private readonly Thread _thread;
        private readonly ICommandRunner _queue;
        private readonly CommandTimer _scheduler;

        public ProcessThread(ICommandRunner queue) : this(queue, "ProcessThread-" + GetNextThreadId())
        {
        }

        public ProcessThread(ICommandRunner queue, string threadName)
        {
            _queue = queue;
            _thread = new Thread(RunThread);
            _thread.Name = threadName;
            _scheduler = new CommandTimer(this);
        }

        public Thread Thread
        {
            get { return _thread; }
        }

        private static int GetNextThreadId()
        {
            return Interlocked.Increment(ref THREAD_COUNT);
        }

        private void RunThread()
        {
            _queue.Run();
        }

        public void Enqueue(Command command)
        {
            _queue.Enqueue(command);
        }

        public void Schedule(Command command, long intervalInMs)
        {
            _scheduler.Schedule(command, intervalInMs);
        }

        public void ScheduleOnInterval(Command command, long firstInMs, long intervalInMs)
        {
            _scheduler.ScheduleOnInterval(command, firstInMs, intervalInMs);
        }

        public void Stop()
        {
            _queue.Stop();
        }

        public void Start()
        {
            _thread.Start();
        }

        public void Join()
        {
            _thread.Join();
        }
    }
}