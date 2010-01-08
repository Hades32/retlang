using System;
using System.Collections.Generic;
using Retlang.Core;

namespace Retlang.Fibers
{
    ///<summary>
    /// For use only in testing.  Allows for controlled execution of scheduled actions on the StubFiber.
    ///</summary>
    public class StubScheduledAction : ITimerControl
    {
        private readonly Action _action;
        private readonly long _firstIntervalInMs;
        private readonly long _intervalInMs;
        
        private readonly List<StubScheduledAction> _registry;

        ///<summary>
        /// Use for recurring scheduled actions.
        ///</summary>
        ///<param name="action"></param>
        ///<param name="firstIntervalInMs"></param>
        ///<param name="intervalInMs"></param>
        ///<param name="registry"></param>
        public StubScheduledAction(Action action, long firstIntervalInMs, long intervalInMs, List<StubScheduledAction> registry)
        {
            _action = action;
            _firstIntervalInMs = firstIntervalInMs;
            _intervalInMs = intervalInMs;
            _registry = registry;
        }

        ///<summary>
        /// Use for scheduled actions that only occur once.
        ///</summary>
        ///<param name="action"></param>
        ///<param name="timeTilEnqueueInMs"></param>
        ///<param name="registry"></param>
        public StubScheduledAction(Action action, long timeTilEnqueueInMs, List<StubScheduledAction> registry)
            : this(action, timeTilEnqueueInMs, -1, registry)
        {
        }

        ///<summary>
        /// First interval in milliseconds.
        ///</summary>
        public long FirstIntervalInMs
        {
            get { return _firstIntervalInMs; }
        }

        ///<summary>
        /// Recurring interval in milliseconds.
        ///</summary>
        public long IntervalInMs
        {
            get { return _intervalInMs; }
        }

        ///<summary>
        /// Executes the scheduled action.  If the action is not recurring it will be removed from the registry.
        ///</summary>
        public void Execute()
        {
            _action();
            if (_intervalInMs == -1)
            {
                Cancel();
            }
        }

        /// <summary>
        /// Cancels scheduled action.  Removes scheduled action from registry.
        /// </summary>
        public void Cancel()
        {
            _registry.Remove(this);
        }
    }
}
