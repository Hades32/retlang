using System.Collections.Generic;
using Retlang.Core;

namespace Retlang.Fibers
{
    internal class SynchronousTimerCommand : TimerCommand
    {
        private readonly List<ScheduledEvent> _scheduled;
        private readonly ScheduledEvent _victim;

        public SynchronousTimerCommand(Command command, long firstIntervalInMs, 
            long intervalInMs, List<ScheduledEvent> scheduled, ScheduledEvent victim) 
            : base(command, firstIntervalInMs, intervalInMs)
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
