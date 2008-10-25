using System;
using System.Collections.Generic;
using System.Threading;

namespace Retlang.Core
{
    /// <summary>
    /// Command delegate.
    /// </summary>
    public delegate void Command();

    /// <summary>
    /// Default implementation.
    /// </summary>
    public class CommandQueue : ICommandExecutor
    {
        private readonly DisposableList _disposables = new DisposableList();

        private readonly object _lock = new object();
        private bool _running = true;
        private int _maxQueueDepth = -1;
        private int _maxEnqueueWaitTime;

        private readonly List<Command> _commands = new List<Command>();

        private IBatchExecutor _batchRunner = new BatchExecutor();

        /// <summary>
        /// Executor for events.
        /// </summary>
        public IBatchExecutor Executor
        {
            get { return _batchRunner; }
            set { _batchRunner = value; }
        }

        /// <summary>
        /// Max number of events to be queued.
        /// </summary>
        public int MaxDepth
        {
            get { return _maxQueueDepth; }
            set { _maxQueueDepth = value; }
        }

        /// <summary>
        /// Max time to wait for space in the queue.
        /// </summary>
        public int MaxEnqueueWaitTime
        {
            get { return _maxEnqueueWaitTime; }
            set { _maxEnqueueWaitTime = value; }
        }

        /// <summary>
        /// <see cref="IDisposingExecutor.EnqueueAll(Command[])"/>
        /// </summary>
        /// <param name="commands"></param>
        public void EnqueueAll(params Command[] commands)
        {
            lock (_lock)
            {
                if (SpaceAvailable(commands.Length))
                {
                    _commands.AddRange(commands);
                    Monitor.PulseAll(_lock);
                }
            }
        }

        /// <summary>
        /// Queue command.
        /// </summary>
        /// <param name="command"></param>
        public void Enqueue(Command command)
        {
            lock (_lock)
            {
                if (SpaceAvailable(1))
                {
                    _commands.Add(command);
                    Monitor.PulseAll(_lock);
                }
            }
        }

        /// <summary>
        /// Add disposable.
        /// </summary>
        /// <param name="toAdd"></param>
        public void Add(IDisposable toAdd)
        {
            _disposables.Add(toAdd);
        }

        /// <summary>
        /// Remove
        /// </summary>
        /// <param name="victim"></param>
        /// <returns></returns>
        public bool Remove(IDisposable victim)
        {
            return _disposables.Remove(victim);
        }

        /// <summary>
        /// Disposable Count.
        /// </summary>
        public int DisposableCount
        {
            get { return _disposables.Count; }
        }

        private bool SpaceAvailable(int toAdd)
        {
            if (!_running)
            {
                return false;
            }
            while (_maxQueueDepth > 0 && _commands.Count + toAdd > _maxQueueDepth)
            {
                if (_maxEnqueueWaitTime <= 0)
                {
                    throw new QueueFullException(_commands.Count);
                }
                Monitor.Wait(_lock, _maxEnqueueWaitTime);
                if (!_running)
                {
                    return false;
                }
                if (_maxQueueDepth > 0 && _commands.Count + toAdd > _maxQueueDepth)
                {
                    throw new QueueFullException(_commands.Count);
                }
            }
            return true;
        }

        /// <summary>
        /// Remove all commands.
        /// </summary>
        /// <returns></returns>
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
                return null;
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

        /// <summary>
        /// Remove all commands and execute.
        /// </summary>
        /// <returns></returns>
        public bool ExecuteNextBatch()
        {
            Command[] toExecute = DequeueAll();
            if (toExecute == null)
            {
                return false;
            }
            _batchRunner.ExecuteAll(toExecute);
            return true;
        }

        /// <summary>
        /// Execute commands until stopped.
        /// </summary>
        public void Run()
        {
            while (ExecuteNextBatch())
            {
            }
        }

        /// <summary>
        /// Stop consuming events.
        /// </summary>
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