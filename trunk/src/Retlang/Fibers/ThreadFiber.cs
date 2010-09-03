using System;
using System.Threading;
using Retlang.Core;

namespace Retlang.Fibers
{
    /// <summary>
    /// Fiber implementation backed by a dedicated thread.
    /// <see cref="IFiber"/>
    /// </summary>
    public class ThreadFiber : IFiber
    {
        private static int THREAD_COUNT;
        private readonly Subscriptions _subscriptions = new Subscriptions();

        private readonly Thread _thread;
        private readonly IQueue _executor;
        private readonly Scheduler _scheduler;

        /// <summary>
        /// Create a thread fiber with the default action executor.
        /// </summary>
        public ThreadFiber() 
            : this(new DefaultQueue())
        {}

        /// <summary>
        /// Creates a thread fiber with a specified executor.
        /// </summary>
        /// <param name="executor"></param>
        public ThreadFiber(IQueue executor) 
            : this(executor, "ThreadFiber-" + GetNextThreadId())
        {}

        /// <summary>
        /// Creates a thread fiber with a specified name.
        /// </summary>
        /// /// <param name="threadName"></param>
        public ThreadFiber(string threadName)
            : this(new DefaultQueue(), threadName)
        {}


        /// <summary>
        /// Creates a thread fiber.
        /// </summary>
        /// <param name="executor"></param>
        /// <param name="threadName"></param>
        /// <param name="isBackground"></param>
        /// <param name="priority"></param>
        public ThreadFiber(IQueue executor, string threadName, bool isBackground = true, ThreadPriority priority = ThreadPriority.Normal)
        {
            _executor = executor;
            _thread = new Thread(RunThread);
            _thread.Name = threadName;
            _thread.IsBackground = isBackground;
            _thread.Priority = priority;
            _scheduler = new Scheduler(this);
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
            _executor.Run();
        }

        /// <summary>
        /// Queue action.
        /// </summary>
        /// <param name="action"></param>
        public void Enqueue(Action action)
        {
            _executor.Enqueue(action);
        }

        /// <summary>
        /// Add Disposable to be invoked when Fiber is disposed.
        /// </summary>
        /// <param name="subscription"></param>
        public void RegisterSubscription(IDisposable subscription)
        {
            _subscriptions.Add(subscription);
        }

        /// <summary>
        /// Remove disposable.
        /// </summary>
        /// <param name="subscription"></param>
        /// <returns></returns>
        public bool DeregisterSubscription(IDisposable subscription)
        {
            return _subscriptions.Remove(subscription);
        }

        /// <summary>
        /// Number of disposables.
        /// </summary>
        public int NumSubscriptions
        {
            get { return _subscriptions.Count; }
        }

        /// <summary>
        /// <see cref="IScheduler.Schedule(Action,long)"/>
        /// </summary>
        /// <param name="action"></param>
        /// <param name="firstInMs"></param>
        /// <returns></returns>
        public IDisposable Schedule(Action action, long firstInMs)
        {
            return _scheduler.Schedule(action, firstInMs);
        }

        /// <summary>
        /// <see cref="IScheduler.ScheduleOnInterval(Action,long,long)"/>
        /// </summary>
        /// <param name="action"></param>
        /// <param name="firstInMs"></param>
        /// <param name="regularInMs"></param>
        public IDisposable ScheduleOnInterval(Action action, long firstInMs, long regularInMs)
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

        ///<summary>
        /// Calls join on the thread.
        ///</summary>
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
            _subscriptions.Dispose();
            _executor.Stop();
        }
    }
}