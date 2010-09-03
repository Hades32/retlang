using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Retlang.Core;

namespace Retlang.Fibers
{
    ///<summary>
    /// Allows interaction with Windows Forms.  Transparently moves actions onto the Form's thread.
    ///</summary>
    public class BaseFiber : IFiber
    {
        private readonly Subscriptions _subscriptions = new Subscriptions();
        private readonly object _lock = new object();
        private readonly IContext _context;
        private readonly Scheduler _timer;
        private readonly IExecutor _executor;
        private readonly List<Action> _queue = new List<Action>();

        private volatile ExecutionState _started = ExecutionState.Created;

        /// <summary>
        /// Creates an instance.
        /// </summary>
        public BaseFiber(IContext context, IExecutor executor)
        {
            _timer = new Scheduler(this);
            _context = context;
            _executor = executor;
        }
        
        /// <summary>
        /// <see cref="IContext.Enqueue(Action)"/>
        /// </summary>
        public void Enqueue(Action action)
        {
            if (_started == ExecutionState.Stopped)
            {
                return;
            }

            if (_started == ExecutionState.Created)
            {
                lock (_lock)
                {
                    if (_started == ExecutionState.Created)
                    {
                        _queue.Add(action);
                        return;
                    }
                }
            }

            _context.Enqueue(() => _executor.ExecuteAll(new List<Action> { action }));
        }

        ///<summary>
        /// Register unsubscriber to be called when IFiber is disposed
        ///</summary>
        ///<param name="toAdd"></param>
        public void RegisterSubscription(IDisposable toAdd)
        {
            _subscriptions.Add(toAdd);
        }

        ///<summary>
        /// Deregister a subscription
        ///</summary>
        ///<param name="toRemove"></param>
        ///<returns></returns>
        public bool DeregisterSubscription(IDisposable toRemove)
        {
            return _subscriptions.Remove(toRemove);
        }

        ///<summary>
        /// Number of subscriptions
        ///</summary>
        public int NumSubscriptions
        {
            get { return _subscriptions.Count; }
        }

        /// <summary>
        /// <see cref="IScheduler.Schedule(Action,long)"/>
        /// </summary>
        public IDisposable Schedule(Action action, long firstInMs)
        {
            return _timer.Schedule(action, firstInMs);
        }

        /// <summary>
        /// <see cref="IScheduler.ScheduleOnInterval(Action,long,long)"/>
        /// </summary>
        public IDisposable ScheduleOnInterval(Action action, long firstInMs, long regularInMs)
        {
            return _timer.ScheduleOnInterval(action, firstInMs, regularInMs);
        }

        /// <summary>
        /// <see cref="IFiber.Start()"/>
        /// </summary>
        public void Start()
        {
            if (_started == ExecutionState.Running)
            {
                throw new ThreadStateException("Already Started");
            }

            lock (_lock)
            {
                var actions = _queue.ToList();
                _queue.Clear();
                if (actions.Count > 0)
                {
                    _context.Enqueue(() => _executor.ExecuteAll(actions));
                }
                _started = ExecutionState.Running;
            }
        }

        /// <summary>
        /// <see cref="IDisposable.Dispose()"/>
        /// </summary>
        public void Dispose()
        {
            Stop();
        }

        /// <summary>
        /// Stops the fiber.
        /// </summary>
        public void Stop()
        {
            _timer.Dispose();
            _started = ExecutionState.Stopped;
            _subscriptions.Dispose();
        }
    }
}