using System;
using System.Collections.Generic;
using System.Linq;

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
        /// Clears all subscriptions, scheduled, and pending actions.
        /// </summary>
        public void Dispose()
        {
            _scheduled.ToList().ForEach(x => x.Dispose());
            _scheduled.Clear();

            _subscriptions.ToList().ForEach(x => x.Dispose());
            _subscriptions.Clear();

            _pending.Clear();
        }

        /// <summary>
        /// Enqueue a single action.
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

        ///<summary>
        /// Register subscription to be unsubcribed from when the fiber is disposed.
        ///</summary>
        ///<param name="toAdd"></param>
        public void RegisterSubscription(IDisposable toAdd)
        {
            _subscriptions.Add(toAdd);
        }

        ///<summary>
        /// Deregister a subscription.
        ///</summary>
        ///<param name="toRemove"></param>
        ///<returns></returns>
        public bool DeregisterSubscription(IDisposable toRemove)
        {
            return _subscriptions.Remove(toRemove);
        }

        ///<summary>
        /// Number of subscriptions.
        ///</summary>
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
