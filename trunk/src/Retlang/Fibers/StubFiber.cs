using System;
using System.Collections.Generic;
using Retlang.Core;

namespace Retlang.Fibers
{
    /// <summary>
    /// StubFiber does not use a backing thread or a thread pool for execution. Actions are added to pending
    /// lists for execution. These actions can be executed synchronously by the calling thread. This class
    /// is not thread safe and should not be used in production code. 
    /// 
    /// The class is typically used for testing asynchronous code to make it completely synchronous and
    /// deterministic.
    /// </summary>
    public class StubFiber : IFiber
    {
        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        private readonly List<Action> _pending = new List<Action>();
        private readonly List<StubScheduledAction> _scheduled = new List<StubScheduledAction>();

        private bool _root = true;

        ///<summary>
        /// Default constructor.  ExecutePendingImmediately is defaulted to true.
        ///</summary>
        public StubFiber()
        {
            ExecutePendingImmediately = true;
        }

        /// <summary>
        /// No Op
        /// </summary>
        public void Start()
        {}

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
            foreach (var action in actions)
            {
                Enqueue(action);
            }
        }

        /// <summary>
        /// Add event to pending list.
        /// </summary>
        /// <param name="action"></param>
        public void Enqueue(Action action)
        {
            if (_root && ExecutePendingImmediately)
            {
                try
                {
                    _root = false;
                    action();
                    ExecuteAllPendingUntilEmpty();
                }
                finally
                {
                    _root = true;
                }
            }
            else
            {
                _pending.Add(action);
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
            var toAdd = new StubScheduledAction(action, timeTilEnqueueInMs, _scheduled);
            _scheduled.Add(toAdd);
            return toAdd;
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
            var toAdd = new StubScheduledAction(action, firstInMs, regularInMs, _scheduled);
            _scheduled.Add(toAdd);
            return toAdd;
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
        public List<StubScheduledAction> Scheduled
        {
            get { return _scheduled; }
        }

        /// <summary>
        /// If true events will be executed immediately rather than added to a pending list.
        /// </summary>
        public bool ExecutePendingImmediately { get; set; }

        /// <summary>
        /// Execute all actions in the pending list.  If any of the executed actions enqueue more actions, execute those as well.
        /// </summary>
        public void ExecuteAllPendingUntilEmpty()
        {
            while (_pending.Count > 0)
            {
                _pending[0]();
                _pending.RemoveAt(0);
            }
        }

        /// <summary>
        /// Execute all actions in the scheduled list.  If any of the executed actions enqueue more actions, execute those as well.
        /// </summary>
        public void ExecuteAllScheduledUntilEmpty()
        {
            while (_scheduled.Count > 0)
            {
                _scheduled[0].Execute();
            }
        }

        /// <summary>
        /// Execute all actions in the pending list.
        /// </summary>
        public void ExecuteAllPending()
        {
            foreach (var pending in _pending.ToArray())
            {
                pending();
            }
            _pending.Clear();
        }

        /// <summary>
        /// Execute all actions in the scheduled list.
        /// </summary>
        public void ExecuteAllScheduled()
        {
            foreach (var scheduled in _scheduled.ToArray())
            {
                scheduled.Execute();
            }
        }
    }
}
