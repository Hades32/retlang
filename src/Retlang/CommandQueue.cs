using System;
using System.Collections.Generic;
using System.Threading;

namespace Retlang
{
    public delegate void Command();

    public delegate void OnException(Command command, Exception failure);

    /// <summary>
    /// Queue for command objects.
    /// </summary>
    public interface ICommandQueue
    {
        /// <summary>
        /// Append command to end of queue.
        /// </summary>
        /// <param name="command"></param>
        void Enqueue(Command command);
    }

    public interface ICommandRunner : ICommandQueue
    {
        void Run();
        void Stop();
    }

    public class CommandQueue : ICommandRunner
    {
        private readonly object _lock = new object();
        private bool _running = true;
        private int _maxQueueDepth = -1;
        private int _maxEnqueueWaitTime = 0;

        private readonly List<Command> _commands = new List<Command>();

        private ICommandExecutor _commandRunner = new CommandExecutor();

        public ICommandExecutor Executor
        {
            get { return _commandRunner; }
            set { _commandRunner = value; }
        }

        public int MaxDepth
        {
            get { return _maxQueueDepth; }
            set { _maxQueueDepth = value; }
        }

        public int MaxEnqueueWaitTime
        {
            get { return _maxEnqueueWaitTime; }
            set { _maxEnqueueWaitTime = value; }
        }

        public void Enqueue(Command command)
        {
            lock (_lock)
            {
                if (SpaceAvailable())
                {
                    _commands.Add(command);
                    Monitor.PulseAll(_lock);
                }
            }
        }

        private bool SpaceAvailable()
        {
            if (!_running)
            {
                return false;
            }
            while (_maxQueueDepth > 0 && _commands.Count >= _maxQueueDepth)
            {
                if (_maxEnqueueWaitTime <= 0)
                {
                    throw new QueueFullException(_commands.Count);
                }
                else
                {
                    Monitor.Wait(_lock, _maxEnqueueWaitTime);
                    if (!_running)
                    {
                        return false;
                    }
                    if (_maxQueueDepth > 0 && _commands.Count >= _maxQueueDepth)
                    {
                        throw new QueueFullException(_commands.Count);
                    }
                }
            }
            return true;
        }

        public Command[] DequeueAll()
        {
            lock (_lock)
            {
                if (ReadyToDequeue())
                {
                    Command[] toReturn = _commands.ToArray();
                    _commands.Clear();
                    Monitor.PulseAll(_lock);
                    return toReturn;
                }
                else
                {
                    return null;
                }
            }
        }

        private bool ReadyToDequeue()
        {
            while (_commands.Count == 0 && _running)
            {
                Monitor.Wait(_lock);
            }
            if (!_running)
            {
                return false;
            }
            return true;
        }

        public bool ExecuteNextBatch()
        {
            Command[] toExecute = DequeueAll();
            if (toExecute == null)
            {
                return false;
            }
            _commandRunner.ExecuteAll(toExecute);
            return true;
        }

        public void Run()
        {
            while (ExecuteNextBatch())
            {
            }
        }

        public void Stop()
        {
            lock (_lock)
            {
                _running = false;
                Monitor.PulseAll(_lock);
            }
        }
    }
}