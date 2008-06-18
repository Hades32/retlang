using System.Collections.Generic;
using System.Threading;

namespace Retlang
{
    internal enum ExecutionState
    {
        Created,
        Running,
        Stopped
    }

    /// <summary>
    /// Process Queue that uses a thread pool for execution.
    /// </summary>
    public class PoolQueue : IProcessQueue
    {
        private bool _flushPending = false;
        private readonly object _lock = new object();
        private readonly List<Command> _queue = new List<Command>();
        private readonly IThreadPool _pool;
        private readonly CommandTimer _timer;
        private ExecutionState _started = ExecutionState.Created;
        private readonly ICommandExecutor _executor;

        /// <summary>
        /// Construct new instance.
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="executor"></param>
        public PoolQueue(IThreadPool pool, ICommandExecutor executor)
        {
            _timer = new CommandTimer(this);
            _pool = pool;
            _executor = executor;
        }

        /// <summary>
        /// Create a pool queue with the default thread pool and command executor.
        /// </summary>
        public PoolQueue() : this(new DefaultThreadPool(), new CommandExecutor())
        {
        }

        /// <summary>
        /// <see cref="ICommandQueue.Enqueue(Command[])"/>
        /// </summary>
        /// <param name="commands"></param>
        public void Enqueue(params Command[] commands)
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
        /// <see cref="ICommandTimer.Schedule(Command,long)"/>
        /// </summary>
        /// <param name="command"></param>
        /// <param name="firstIntervalInMs"></param>
        /// <returns></returns>
        public ITimerControl Schedule(Command command, long firstIntervalInMs)
        {
            return _timer.Schedule(command, firstIntervalInMs);
        }

        /// <summary>
        /// <see cref="ICommandTimer.ScheduleOnInterval(Command,long,long)"/>
        /// </summary>
        /// <param name="command"></param>
        /// <param name="firstIntervalInMs"></param>
        /// <param name="regularIntervalInMs"></param>
        /// <returns></returns>
        public ITimerControl ScheduleOnInterval(Command command, long firstIntervalInMs, long regularIntervalInMs)
        {
            return _timer.ScheduleOnInterval(command, firstIntervalInMs, regularIntervalInMs);
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
