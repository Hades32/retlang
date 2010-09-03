using System;
using System.Collections.Generic;
using System.Threading;

namespace Retlang.Core
{
    /// <summary>
    /// Busy waits on lock to execute.  Can improve perforamance in certain situations.
    /// </summary>
    public class BusyWaitQueue : IQueue
    {
        private readonly object _lock = new object();
        private readonly IExecutor _executor;
        private readonly int _spinsBeforeSleepCheck;
        private readonly int _sleepInMs;

        private bool _running = true;

        private List<Action> _actions = new List<Action>();
        private List<Action> _toPass = new List<Action>();

        ///<summary>
        /// BusyWaitQueue with custom executor
        ///</summary>
        ///<param name="executor"></param>
        ///<param name="spinsBeforeSleepCheck"></param>
        ///<param name="sleepInMs"></param>
        public BusyWaitQueue(IExecutor executor, int spinsBeforeSleepCheck, int sleepInMs)
        {
            _executor = executor;
            _spinsBeforeSleepCheck = spinsBeforeSleepCheck;
            _sleepInMs = sleepInMs;
        }

        ///<summary>
        /// BusyWaitQueue with default executor
        ///</summary>
        public BusyWaitQueue(int spinsBeforeSleep, int sleepCheckInMs) 
            : this(new DefaultExecutor(), spinsBeforeSleep, sleepCheckInMs)
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
            lock (_lock)
            {
                if (ReadyToDequeue())
                {
                    Lists.Swap(ref _actions, ref _toPass);
                    _actions.Clear();

                    Monitor.PulseAll(_lock);
                    return _toPass;
                }
                return null;
            }
        }

        private bool ReadyToDequeue()
        {
            while (_actions.Count == 0 && _running)
            {
                var spins = 0;
                var sleepInMs = 0;
                while (!Monitor.Wait(_lock, sleepInMs))
                {
                    sleepInMs = 0;
                    if (++spins > _spinsBeforeSleepCheck)
                    {
                        spins = 0;
                        sleepInMs = _sleepInMs;
                    }
                }
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
        private bool ExecuteNextBatch()
        {
            var toExecute = DequeueAll();
            if (toExecute == null)
            {
                return false;
            }
            _executor.ExecuteAll(toExecute);
            return true;
        }
    }
}