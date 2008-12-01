using System;
using System.Collections.Generic;
using Retlang.Core;

namespace Retlang.Fibers
{
    internal class SynchronousTimerAction : TimerAction
    {
        private readonly List<ScheduledEvent> _scheduled;
        private readonly ScheduledEvent _victim;

        public SynchronousTimerAction(Action action, long firstIntervalInMs, 
            long intervalInMs, List<ScheduledEvent> scheduled, ScheduledEvent victim) 
            : base(action, firstIntervalInMs, intervalInMs)
        {
            _scheduled = scheduled;
            _victim = victim;
        }

        public override void Cancel()
        {
            _scheduled.Remove(_victim);
            base.Cancel();
        }
    }
}
