using System;
using System.Collections.Generic;

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
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();
        private readonly List<Action> _pending = new List<Action>();
        private readonly List<StubScheduledAction> _scheduled = new List<StubScheduledAction>();

        private bool _root = true;

        /// <summary>
        /// No Op
        /// </summary>
        public void Start()
        {}

        /// <summary>
        /// Unsubscribes from all subscriptions.
        /// </summary>
        public void Dispose()
        {
            foreach (var subscription in _subscriptions.ToArray())
            {
                subscription.Dispose();
            }

            _subscriptions.Clear();
        }
        
        /// <summary>
        /// Add action to pending list.
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
        public void RegisterSubscription(IDisposable disposable)
        {
            _subscriptions.Add(disposable);
        }

        /// <summary>
        /// Remove Disposable.
        /// </summary>
        /// <param name="disposable"></param>
        /// <returns></returns>
        public bool DeregisterSubscription(IDisposable disposable)
        {
            return _subscriptions.Remove(disposable);
        }

        /// <summary>
        /// Count of Disposables.
        /// </summary>
        public int NumSubscriptions
        {
            get { return _subscriptions.Count; }
        }

        /// <summary>
        /// Adds a scheduled action to the list. 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="firstInMs"></param>
        /// <returns></returns>
        public IDisposable Schedule(Action action, long firstInMs)
        {
            var toAdd = new StubScheduledAction(action, firstInMs, _scheduled);
            _scheduled.Add(toAdd);
            return toAdd;
        }

        /// <summary>
        /// Adds scheduled action to list.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="firstInMs"></param>
        /// <param name="regularInMs"></param>
        /// <returns></returns>
        public IDisposable ScheduleOnInterval(Action action, long firstInMs, long regularInMs)
        {
            var toAdd = new StubScheduledAction(action, firstInMs, regularInMs, _scheduled);
            _scheduled.Add(toAdd);
            return toAdd;
        }

        /// <summary>
        /// All subscriptions.
        /// </summary>
        public List<IDisposable> Subscriptions
        {
            get { return _subscriptions; }
        }

        /// <summary>
        /// All pending actions.
        /// </summary>
        public List<Action> Pending
        {
            get { return _pending; }
        }

        /// <summary>
        /// All scheduled actions.
        /// </summary>
        public List<StubScheduledAction> Scheduled
        {
            get { return _scheduled; }
        }

        /// <summary>
        /// If true events will be executed immediately rather than added to the pending list.
        /// </summary>
        public bool ExecutePendingImmediately { get; set; }

        /// <summary>
        /// Execute all actions in the pending list.  If any of the executed actions enqueue more actions, execute those as well.
        /// </summary>
        public void ExecuteAllPendingUntilEmpty()
        {
            while (_pending.Count > 0)
            {
                var toExecute = _pending[0];
                _pending.RemoveAt(0);
                toExecute();
            }
        }

        /// <summary>
        /// Execute all actions in the pending list.
        /// </summary>
        public void ExecuteAllPending()
        {
            var copy = _pending.ToArray();
            _pending.Clear();
            foreach (var pending in copy)
            {
                pending();
            }
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
