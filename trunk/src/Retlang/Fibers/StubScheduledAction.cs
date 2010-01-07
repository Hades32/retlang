using System;
using System.Collections.Generic;
using Retlang.Core;

namespace Retlang.Fibers
{
    public class StubScheduledAction : ITimerControl
    {
        private readonly Action _action;
        private readonly long _firstIntervalInMs;
        private readonly long _intervalInMs;
        
        private readonly List<StubScheduledAction> _registry;

        public StubScheduledAction(Action action, long firstIntervalInMs, long intervalInMs, List<StubScheduledAction> registry)
        {
            _action = action;
            _firstIntervalInMs = firstIntervalInMs;
            _intervalInMs = intervalInMs;
            _registry = registry;
        }

        public StubScheduledAction(Action action, long timeTilEnqueueInMs, List<StubScheduledAction> registry)
            : this(action, timeTilEnqueueInMs, -1, registry)
        {
        }

        public long FirstIntervalInMs
        {
            get { return _firstIntervalInMs; }
        }

        public long IntervalInMs
        {
            get { return _intervalInMs; }
        }

        public void Execute()
        {
            _action();
            if (_intervalInMs == -1)
            {
                Cancel();
            }
        }

        public void Cancel()
        {
            _registry.Remove(this);
        }
    }
}
