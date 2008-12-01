using System;
using System.Collections.Generic;
using Retlang.Core;

namespace Retlang.Fibers
{
    /// <summary>
    /// Synchronous Fiber does not use a backing thread or a thread pool for execution. Events are added to pending
    /// lists for execution. These events can be executed synchronously by a calling thread. This class
    /// is not thread safe and probably should not be used in production code. 
    /// 
    /// The class is typically used for unit testing asynchronous code to make it completely synchronous and
    /// deterministic.
    /// </summary>
    public class SynchronousFiber : IFiber
    {
        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        private readonly List<Action> _pending = new List<Action>();
        private readonly List<ScheduledEvent> _scheduled = new List<ScheduledEvent>();
        private bool _executePendingImmediately;

        /// <summary>
        /// No Op
        /// </summary>
        public void Start()
        {
        }

        /// <summary>
        /// Invokes Disposables.
        /// </summary>
        public void Dispose()
        {
            foreach (var d in _disposables.ToArray())
            {
                d.Dispose();
            }
        }

        /// <summary>
        /// Adds all events to pending list.
        /// </summary>
        /// <param name="actions"></param>
        public void EnqueueAll(params Action[] actions)
        {
            _pending.AddRange(actions);
            if (_executePendingImmediately)
            {
                ExecuteAllPending();       
            }
        }

        /// <summary>
        /// Add event to pending list.
        /// </summary>
        /// <param name="action"></param>
        public void Enqueue(Action action)
        {
            _pending.Add(action);
            if (_executePendingImmediately)
            {
                ExecuteAllPending();
            }
        }

        /// <summary>
        /// add to disposable list.
        /// </summary>
        /// <param name="disposable"></param>
        public void Add(IDisposable disposable)
        {
            _disposables.Add(disposable);
        }

        /// <summary>
        /// Remove Disposable.
        /// </summary>
        /// <param name="disposable"></param>
        /// <returns></returns>
        public bool Remove(IDisposable disposable)
        {
            return _disposables.Remove(disposable);
        }

        /// <summary>
        /// Count of Disposables.
        /// </summary>
        public int DisposableCount
        {
            get { return _disposables.Count; }
        }

        /// <summary>
        /// Adds a scheduled event to the list. 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="timeTilEnqueueInMs"></param>
        /// <returns></returns>
        public ITimerControl Schedule(Action action, long timeTilEnqueueInMs)
        {
            var toAdd = new ScheduledEvent(action, timeTilEnqueueInMs);
            _scheduled.Add(toAdd);

            return new SynchronousTimerAction(action, timeTilEnqueueInMs, 
                timeTilEnqueueInMs, _scheduled, toAdd);
        }

        /// <summary>
        /// Adds scheduled event to list.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="firstInMs"></param>
        /// <param name="regularInMs"></param>
        /// <returns></returns>
        public ITimerControl ScheduleOnInterval(Action action, long firstInMs, long regularInMs)
        {
            var toAdd = new ScheduledEvent(action, firstInMs, regularInMs);
            _scheduled.Add(toAdd);

            return new SynchronousTimerAction(action, firstInMs,
                regularInMs, _scheduled, toAdd);
        }

        /// <summary>
        /// All Disposables.
        /// </summary>
        public List<IDisposable> Disposables
        {
            get { return _disposables; }
        }

        /// <summary>
        /// All Pending actions.
        /// </summary>
        public List<Action> Pending
        {
            get { return _pending; }
        }

        /// <summary>
        /// All Scheduled events.
        /// </summary>
        public List<ScheduledEvent> Scheduled
        {
            get { return _scheduled; }
        }

        /// <summary>
        /// If true events will be executed immediately rather than added to a pending list.
        /// </summary>
        public bool ExecutePendingImmediately
        {
            get { return _executePendingImmediately; }
            set { _executePendingImmediately = value; }
        }

        /// <summary>
        /// Execute all actions in the pending list.
        /// </summary>
        public void ExecuteAllPending()
        {
            foreach (var action in _pending)
            {
                action();
            }
            _pending.Clear();
        }

        /// <summary>
        /// execute all scheduled.
        /// </summary>
        public void ExecuteAllScheduled()
        {
            foreach (var scheduledEvent in _scheduled)
            {
                scheduledEvent.Action();
            }
            _scheduled.Clear();
        }
    }
}
