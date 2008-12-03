using System;
using System.Collections.Generic;
using System.Threading;

namespace Retlang.Core
{
    /// <summary>
    /// Default implementation.
    /// </summary>
    public class ActionExecutor : IActionExecutor
    {
        private readonly DisposableList _disposables = new DisposableList();

        private readonly object _lock = new object();
        private bool _running = true;
        private int _maxQueueDepth = -1;
        private int _maxEnqueueWaitTime;

        private readonly List<Action> _actions = new List<Action>();

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
        /// <see cref="IDisposingExecutor.EnqueueAll(Action[])"/>
        /// </summary>
        /// <param name="actions"></param>
        public void EnqueueAll(params Action[] actions)
        {
            lock (_lock)
            {
                if (SpaceAvailable(actions.Length))
                {
                    _actions.AddRange(actions);
                    Monitor.PulseAll(_lock);
                }
            }
        }

        /// <summary>
        /// Queue action.
        /// </summary>
        /// <param name="action"></param>
        public void Enqueue(Action action)
        {
            lock (_lock)
            {
                if (SpaceAvailable(1))
                {
                    _actions.Add(action);
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
            while (_maxQueueDepth > 0 && _actions.Count + toAdd > _maxQueueDepth)
            {
                if (_maxEnqueueWaitTime <= 0)
                {
                    throw new QueueFullException(_actions.Count);
                }
                Monitor.Wait(_lock, _maxEnqueueWaitTime);
                if (!_running)
                {
                    return false;
                }
                if (_maxQueueDepth > 0 && _actions.Count + toAdd > _maxQueueDepth)
                {
                    throw new QueueFullException(_actions.Count);
                }
            }
            return true;
        }

        /// <summary>
        /// Remove all actions.
        /// </summary>
        /// <returns></returns>
        public Action[] DequeueAll()
        {
            lock (_lock)
            {
                if (ReadyToDequeue())
                {
                    var toReturn = _actions.ToArray();
                    _actions.Clear();
                    Monitor.PulseAll(_lock);
                    return toReturn;
                }
                return null;
            }
        }

        private bool ReadyToDequeue()
        {
            while (_actions.Count == 0 && _running)
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
        /// Remove all actions and execute.
        /// </summary>
        /// <returns></returns>
        public bool ExecuteNextBatch()
        {
            var toExecute = DequeueAll();
            if (toExecute == null)
            {
                return false;
            }
            _batchRunner.ExecuteAll(toExecute);
            return true;
        }

        /// <summary>
        /// Execute actions until stopped.
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