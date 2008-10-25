using System;
using System.Collections.Generic;
using System.Threading;
using Retlang.Core;

namespace Retlang.Fibers
{
    /// <summary>
    /// Process Queue that uses a thread pool for execution.
    /// </summary>
    public class PoolFiber : IFiber
    {
        private readonly DisposableList _disposables = new DisposableList();
        private readonly object _lock = new object();
        private readonly List<Command> _queue = new List<Command>();
        private readonly IThreadPool _pool;
        private readonly CommandTimer _timer;
        private readonly IBatchExecutor _executor;

        private ExecutionState _started = ExecutionState.Created;
        private bool _flushPending;

        /// <summary>
        /// Construct new instance.
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="executor"></param>
        public PoolFiber(IThreadPool pool, IBatchExecutor executor)
        {
            _timer = new CommandTimer(this);
            _pool = pool;
            _executor = executor;
        }

        /// <summary>
        /// Create a pool queue with the default thread pool and command executor.
        /// </summary>
        public PoolFiber() : this(new DefaultThreadPool(), new BatchExecutor())
        {
        }

        /// <summary>
        /// <see cref="IDisposingExecutor.EnqueueAll(Command[])"/>
        /// </summary>
        /// <param name="commands"></param>
        public void EnqueueAll(params Command[] commands)
        {
            if (_started == ExecutionState.Stopped)
            {
                return;
            }

            lock (_lock)
            {
                _queue.AddRange(commands);
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
        /// Queue command.
        /// </summary>
        /// <param name="commands"></param>
        public void Enqueue(Command commands)
        {
            if (_started == ExecutionState.Stopped)
            {
                return;
            }

            lock (_lock)
            {
                _queue.Add(commands);
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
        public void Add(IDisposable toAdd)
        {
            _disposables.Add(toAdd);
        }

        /// <summary>
        /// Remove Disposable.
        /// </summary>
        /// <param name="victim"></param>
        /// <returns></returns>
        public bool Remove(IDisposable victim)
        {
            return _disposables.Remove(victim);
        }

        /// <summary>
        /// Number of currently registered disposables.
        /// </summary>
        public int DisposableCount
        {
            get { return _disposables.Count; }
        }

        private void Flush(object state)
        {
            Command[] toExecute = ClearCommands();
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

        private Command[] ClearCommands()
        {
            lock (_lock)
            {
                if (_queue.Count == 0)
                {
                    _flushPending = false;
                    return null;
                }
                Command[] toReturn = _queue.ToArray();
                _queue.Clear();
                return toReturn;
            }
        }

        /// <summary>
        /// <see cref="IScheduler.Schedule(Command,long)"/>
        /// </summary>
        /// <param name="command"></param>
        /// <param name="timeTilEnqueueInMs"></param>
        /// <returns></returns>
        public ITimerControl Schedule(Command command, long timeTilEnqueueInMs)
        {
            return _timer.Schedule(command, timeTilEnqueueInMs);
        }

        /// <summary>
        /// <see cref="IScheduler.ScheduleOnInterval(Command,long,long)"/>
        /// </summary>
        /// <param name="command"></param>
        /// <param name="firstInMs"></param>
        /// <param name="regularInMs"></param>
        /// <returns></returns>
        public ITimerControl ScheduleOnInterval(Command command, long firstInMs, long regularInMs)
        {
            return _timer.ScheduleOnInterval(command, firstInMs, regularInMs);
        }

        /// <summary>
        /// Start consuming events.
        /// </summary>
        public void Start()
        {
            if (_started == ExecutionState.Running)
            {
                throw new ThreadStateException("Already Started");
            }
            _started = ExecutionState.Running;
            //flush any pending events in queue
            Enqueue(delegate { });
        }

        /// <summary>
        /// Stop consuming events.
        /// </summary>
        public void Stop()
        {
            _timer.Dispose();
            _started = ExecutionState.Stopped;
        }

        /// <summary>
        /// Stops the queue.
        /// </summary>
        public void Dispose()
        {
            Stop();
        }
    }
}