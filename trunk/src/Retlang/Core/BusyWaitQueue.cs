using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Retlang.Core
{
    /// <summary>
    /// Busy waits on lock to execute.  Can improve performance in certain situations.
    /// </summary>
    public class BusyWaitQueue : IQueue
    {
        private readonly object _lock = new object();
        private readonly IExecutor _executor;
        private readonly int _spinsBeforeTimeCheck;
        private readonly int _msBeforeRealWait;

        private bool _running = true;

        private List<Action> _actions = new List<Action>();
        private List<Action> _toPass = new List<Action>();

        private int _spins;
        private readonly Stopwatch _stopwatch = new Stopwatch();

        ///<summary>
        /// BusyWaitQueue with custom executor
        ///</summary>
        ///<param name="executor"></param>
        ///<param name="spinsBeforeTimeCheck"></param>
        ///<param name="msBeforeRealWait"></param>
        public BusyWaitQueue(IExecutor executor, int spinsBeforeTimeCheck, int msBeforeRealWait)
        {
            _executor = executor;
            _spinsBeforeTimeCheck = spinsBeforeTimeCheck;
            _msBeforeRealWait = msBeforeRealWait;
        }

        ///<summary>
        /// BusyWaitQueue with default executor
        ///</summary>
        public BusyWaitQueue(int spinsBeforeSleep, int timeCheckInMs) 
            : this(new DefaultExecutor(), spinsBeforeSleep, timeCheckInMs)
        {
        }
        
        /// <summary>
        /// Enqueue action.
        /// </summary>
        /// <param name="action"></param>
        public void Enqueue(Action action)
        {
            lock (_lock)
            {
                _actions.Add(action);
                Monitor.PulseAll(_lock);
            }
        }

        /// <summary>
        /// Execute actions until stopped.
        /// </summary>
        public void Run()
        {
            while (ExecuteNextBatch()) {}
        }

        /// <summary>
        /// Stop consuming actions.
        /// </summary>
        public void Stop()
        {
            lock (_lock)
            {
                _running = false;
                Monitor.PulseAll(_lock);
            }
        }
        
        private List<Action> DequeueAll()
        {
            _spins = 0;
            _stopwatch.Restart();
            
            while (true)
            {
                try
                {
                    while (!Monitor.TryEnter(_lock)) {}

                    if (!_running) break;
                    var toReturn = TryDequeue();
                    if (toReturn != null) return toReturn;

                    if (TimeForRealWait())
                    {
                        if (!_running) break;
                        toReturn = TryDequeue();
                        if (toReturn != null) return toReturn;
                    }
                }
                finally
                {
                    _stopwatch.Stop();
                    Monitor.Exit(_lock);
                }
            }

            return null;
        }

        private bool TimeForRealWait()
        {
            if (_spins++ <= _spinsBeforeTimeCheck)
            {
                return false;
            }

            _spins = 0;
            _stopwatch.Stop();
            if (_stopwatch.ElapsedMilliseconds > _msBeforeRealWait)
            {
                Monitor.Wait(_lock);
                _stopwatch.Restart();
                return true;
            }

            _stopwatch.Start();
            return false;
        }

        private List<Action> TryDequeue()
        {
            if (_actions.Count > 0)
            {
                Lists.Swap(ref _actions, ref _toPass);
                _actions.Clear();

                Monitor.PulseAll(_lock);
                return _toPass;
            }

            return null;
        }

        /// <summary>
        /// Remove all actions and execute.
        /// </summary>
        /// <returns></returns>
        private bool ExecuteNextBatch()
        {
            var toExecute = DequeueAll();
            if (toExecute == null)
            {
                return false;
            }
            _executor.Execute(toExecute);
            return true;
        }
    }
}