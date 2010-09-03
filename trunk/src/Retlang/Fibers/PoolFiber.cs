using System;
using System.Collections.Generic;
using System.Threading;
using Retlang.Core;

namespace Retlang.Fibers
{
    /// <summary>
    /// Fiber that uses a thread pool for execution.
    /// </summary>
    public class PoolFiber : IFiber
    {
        private readonly Subscriptions _subscriptions = new Subscriptions();
        private readonly object _lock = new object();
        private readonly IThreadPool _pool;
        private readonly Scheduler _timer;
        private readonly IExecutor _executor;

        private List<Action> _queue = new List<Action>();
        private List<Action> _toPass = new List<Action>();

        private ExecutionState _started = ExecutionState.Created;
        private bool _flushPending;

        /// <summary>
        /// Construct new instance.
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="executor"></param>
        public PoolFiber(IThreadPool pool, IExecutor executor)
        {
            _timer = new Scheduler(this);
            _pool = pool;
            _executor = executor;
        }

        /// <summary>
        /// Create a pool fiber with the default thread pool.
        /// </summary>
        public PoolFiber(IExecutor executor) 
            : this(new DefaultThreadPool(), executor)
        {
        }

        /// <summary>
        /// Create a pool fiber with the default thread pool and default executor.
        /// </summary>
        public PoolFiber() 
            : this(new DefaultThreadPool(), new DefaultExecutor())
        {
        }
        
        /// <summary>
        /// Queue action.
        /// </summary>
        /// <param name="action"></param>
        public void Enqueue(Action action)
        {
            if (_started == ExecutionState.Stopped)
            {
                return;
            }

            lock (_lock)
            {
                _queue.Add(action);
                if (_started == ExecutionState.Created)
                {
                    return;
                }
                if (!_flushPending)
                {
                    _pool.Queue(Flush);
                    _flushPending = true;
                }
            }
        }

        /// <summary>
        /// Register Disposable.
        /// </summary>
        /// <param name="toAdd"></param>
        public void RegisterSubscription(IDisposable toAdd)
        {
            _subscriptions.Add(toAdd);
        }

        /// <summary>
        /// Remove Disposable.
        /// </summary>
        /// <param name="toRemove"></param>
        /// <returns></returns>
        public bool DeregisterSubscription(IDisposable toRemove)
        {
            return _subscriptions.Remove(toRemove);
        }

        /// <summary>
        /// Number of currently registered subscription.
        /// </summary>
        public int NumSubscriptions
        {
            get { return _subscriptions.Count; }
        }

        private void Flush(object state)
        {
            var toExecute = ClearActions();
            if (toExecute != null)
            {
                _executor.ExecuteAll(toExecute);
                lock (_lock)
                {
                    if (_queue.Count > 0)
                    {
                        // don't monopolize thread.
                        _pool.Queue(Flush);
                    }
                    else
                    {
                        _flushPending = false;
                    }
                }
            }
        }

        private List<Action> ClearActions()
        {
            lock (_lock)
            {
                if (_queue.Count == 0)
                {
                    _flushPending = false;
                    return null;
                }
                Lists.Swap(ref _queue, ref _toPass);
                _queue.Clear();
                return _toPass;
            }
        }

        /// <summary>
        /// <see cref="IScheduler.Schedule(Action,long)"/>
        /// </summary>
        /// <param name="action"></param>
        /// <param name="firstInMs"></param>
        /// <returns></returns>
        public IDisposable Schedule(Action action, long firstInMs)
        {
            return _timer.Schedule(action, firstInMs);
        }

        /// <summary>
        /// <see cref="IScheduler.ScheduleOnInterval(Action,long,long)"/>
        /// </summary>
        /// <param name="action"></param>
        /// <param name="firstInMs"></param>
        /// <param name="regularInMs"></param>
        /// <returns></returns>
        public IDisposable ScheduleOnInterval(Action action, long firstInMs, long regularInMs)
        {
            return _timer.ScheduleOnInterval(action, firstInMs, regularInMs);
        }

        /// <summary>
        /// Start consuming actions.
        /// </summary>
        public void Start()
        {
            if (_started == ExecutionState.Running)
            {
                throw new ThreadStateException("Already Started");
            }
            _started = ExecutionState.Running;
            //flush any pending events in queue
            Enqueue(() => { });
        }

        /// <summary>
        /// Stop consuming actions.
        /// </summary>
        public void Stop()
        {
            _timer.Dispose();
            _started = ExecutionState.Stopped;
            _subscriptions.Dispose();
        }

        /// <summary>
        /// Stops the fiber.
        /// </summary>
        public void Dispose()
        {
            Stop();
        }
    }
}