using System.Threading;

namespace Retlang
{
    /// <summary>
    /// Queues pending events for the process.
    /// </summary>
    public interface IProcessQueue : ICommandQueue, ICommandTimer
    {
        /// <summary>
        /// Start consuming events.
        /// </summary>
        void Start();
        /// <summary>
        /// Stop consuming events.
        /// </summary>
        void Stop();
    }

    /// <summary>
    /// A process queue backed by a thread.
    /// </summary>
    public interface IProcessThread : IProcessQueue
    {
        /// <summary>
        /// The backing thead.
        /// </summary>
        Thread Thread { get; }

        /// <summary>
        /// Wait for the thread to complete.
        /// </summary>
        void Join();
    }

    /// <summary>
    /// Default implementation for IProcessThread.
    /// <see cref="IProcessThread"/>
    /// </summary>
    public class ProcessThread : IProcessThread
    {
        private static int THREAD_COUNT = 0;

        private readonly Thread _thread;
        private readonly ICommandRunner _queue;
        private readonly CommandTimer _scheduler;

        /// <summary>
        /// Creates a new thread with the backing runner.
        /// </summary>
        /// <param name="queue"></param>
        public ProcessThread(ICommandRunner queue) : this(queue, "ProcessThread-" + GetNextThreadId(), true)
        {
        }

        /// <summary>
        /// Create a process thread with a default queue.
        /// </summary>
        public ProcessThread(): this(new CommandQueue())
        {
            
        }

        /// <summary>
        /// Creates a new thread.
        /// </summary>
        /// <param name="queue">The queue</param>
        /// <param name="threadName">custom thread name</param>
        public ProcessThread(ICommandRunner queue, string threadName)
            : this(queue, threadName, true)
        {
        }

        /// <summary>
        /// Create process thread.
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="threadName"></param>
        /// <param name="isBackground"></param>
        public ProcessThread(ICommandRunner queue, string threadName, bool isBackground)
        {
            _queue = queue;
            _thread = new Thread(RunThread);
            _thread.Name = threadName;
            _thread.IsBackground = isBackground;
            _scheduler = new CommandTimer(this);
        }

        /// <summary>
        /// <see cref="IProcessThread.Thread"/>
        /// </summary>
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

        /// <summary>
        /// <see cref="ICommandQueue.Enqueue(Command)"/>
        /// </summary>
        /// <param name="command"></param>
        public void Enqueue(Command command)
        {
            _queue.Enqueue(command);
        }

        /// <summary>
        /// <see cref="ICommandTimer.Schedule(Command,long)"/>
        /// </summary>
        /// <param name="command"></param>
        /// <param name="intervalInMs"></param>
        /// <returns></returns>
        public ITimerControl Schedule(Command command, long intervalInMs)
        {
            return _scheduler.Schedule(command, intervalInMs);
        }

        /// <summary>
        /// <see cref="ICommandTimer.ScheduleOnInterval(Command,long,long)"/>
        /// </summary>
        /// <param name="command"></param>
        /// <param name="firstInMs"></param>
        /// <param name="intervalInMs"></param>
        public ITimerControl ScheduleOnInterval(Command command, long firstInMs, long intervalInMs)
        {
            return _scheduler.ScheduleOnInterval(command, firstInMs, intervalInMs);
        }

        /// <summary>
        /// <see cref="IProcessQueue.Stop"/>
        /// </summary>
        public void Stop()
        {
            _queue.Stop();
        }

        /// <summary>
        /// <see cref="IProcessQueue.Start"/>
        /// </summary>
        public void Start()
        {
            _thread.Start();
        }

        /// <summary>
        /// <see cref="IProcessThread.Join"/>
        /// </summary>
        public void Join()
        {
            _thread.Join();
        }
    }
}