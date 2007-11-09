using System.Collections.Generic;

namespace Retlang
{
    public class PoolQueue : IProcessQueue
    {
        private bool _flushPending = false;
        private readonly object _lock = new object();
        private readonly List<Command> _queue = new List<Command>();
        private readonly IThreadPool _pool;
        private readonly CommandTimer _timer;
        private bool _started;
        private readonly ICommandExecutor _executor;

        public PoolQueue(IThreadPool pool, ICommandExecutor executor)
        {
            _timer = new CommandTimer(this);
            _pool = pool;
            _executor = executor;
        }

        public void Enqueue(Command command)
        {
            if (!_started)
            {
                return;
            }

            lock (_lock)
            {
                _queue.Add(command);
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

        public void Schedule(Command command, int firstIntervalInMs)
        {
            _timer.Schedule(command, firstIntervalInMs);
        }

        public void ScheduleOnInterval(Command command, int firstIntervalInMs, int regularIntervalInMs)
        {
            _timer.ScheduleOnInterval(command, firstIntervalInMs, regularIntervalInMs);
        }

        public void Start()
        {
            _started = true;
        }

        public void Stop()
        {
            _started = false;
        }

        public void Join()
        {
        }
    }
}