using System;
using System.Threading;
using Retlang.Core;

namespace Retlang.Fibers
{
    /// <summary>
    /// Default implementation for IProcessThread.
    /// <see cref="IFiber"/>
    /// </summary>
    public class ThreadFiber : IThreadFiber
    {
        private static int THREAD_COUNT;
        private readonly DisposableList _disposables = new DisposableList();

        private readonly Thread _thread;
        private readonly IActionExecutor _queue;
        private readonly ActionTimer _scheduler;

        /// <summary>
        /// Creates a new thread with the backing executor.
        /// </summary>
        /// <param name="executor"></param>
        public ThreadFiber(IActionExecutor executor) : this(executor, "ThreadFiber-" + GetNextThreadId(), true)
        {}

        /// <summary>
        /// Create a process thread with a default queue.
        /// </summary>
        public ThreadFiber() : this(new ActionExecutor())
        {}

        /// <summary>
        /// Creates a new thread.
        /// </summary>
        /// <param name="executor">The queue</param>
        /// <param name="threadName">custom thread name</param>
        public ThreadFiber(IActionExecutor executor, string threadName)
            : this(executor, threadName, true)
        {}

        /// <summary>
        /// Create process thread.
        /// </summary>
        /// <param name="executor"></param>
        /// <param name="threadName"></param>
        /// <param name="isBackground"></param>
        public ThreadFiber(IActionExecutor executor, string threadName, bool isBackground)
        {
            _queue = executor;
            _thread = new Thread(RunThread);
            _thread.Name = threadName;
            _thread.IsBackground = isBackground;
            _scheduler = new ActionTimer(this);
        }

        /// <summary>
        /// <see cref="IFiber"/>
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
        /// <see cref="IDisposingExecutor.EnqueueAll(Action[])"/>
        /// </summary>
        /// <param name="actions"></param>
        public void EnqueueAll(params Action[] actions)
        {
            _queue.EnqueueAll(actions);
        }

        /// <summary>
        /// Queue action.
        /// </summary>
        /// <param name="action"></param>
        public void Enqueue(Action action)
        {
            _queue.Enqueue(action);
        }

        /// <summary>
        /// Add Disposable to be invoked when Fiber is disposed.
        /// </summary>
        /// <param name="toAdd"></param>
        public void Add(IDisposable toAdd)
        {
            _disposables.Add(toAdd);
        }

        /// <summary>
        /// Remove disposable.
        /// </summary>
        /// <param name="victim"></param>
        /// <returns></returns>
        public bool Remove(IDisposable victim)
        {
            return _disposables.Remove(victim);
        }

        /// <summary>
        /// Number of disposables.
        /// </summary>
        public int DisposableCount
        {
            get { return _disposables.Count; }
        }

        /// <summary>
        /// <see cref="IScheduler.Schedule(Action,long)"/>
        /// </summary>
        /// <param name="action"></param>
        /// <param name="timeTilEnqueueInMs"></param>
        /// <returns></returns>
        public ITimerControl Schedule(Action action, long timeTilEnqueueInMs)
        {
            return _scheduler.Schedule(action, timeTilEnqueueInMs);
        }

        /// <summary>
        /// <see cref="IScheduler.ScheduleOnInterval(Action,long,long)"/>
        /// </summary>
        /// <param name="action"></param>
        /// <param name="firstInMs"></param>
        /// <param name="regularInMs"></param>
        public ITimerControl ScheduleOnInterval(Action action, long firstInMs, long regularInMs)
        {
            return _scheduler.ScheduleOnInterval(action, firstInMs, regularInMs);
        }

        /// <summary>
        /// <see cref="IFiber.Start"/>
        /// </summary>
        public void Start()
        {
            _thread.Start();
        }

        /// <summary>
        /// <see cref="IThreadFiber.Join"/>
        /// </summary>
        public void Join()
        {
            _thread.Join();
        }

        /// <summary>
        /// Stops the thread.
        /// </summary>
        public void Dispose()
        {
            _scheduler.Dispose();
            _queue.Stop();
        }
    }
}