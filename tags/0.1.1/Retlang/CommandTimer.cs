using System;
using System.Collections.Generic;
using System.Threading;

namespace Retlang
{
    public interface IPendingCommandRegistry
    {
        void Remove(IPendingCommand pending);
    }

    public interface IPendingCommand
    {
    }

    public class PendingCommand: IPendingCommand
    {
        private readonly OnCommand _command;
        private readonly int _firstIntervalInMs;
        private readonly int _intervalInMs;

        private Timer _timer;

        public PendingCommand(OnCommand command, int firstIntervalInMs, int intervalInMs)
        {
            _command = command;
            _firstIntervalInMs = firstIntervalInMs;
            _intervalInMs = intervalInMs;
        }

        public void Schedule(IPendingCommandRegistry registry)
        {
            OnCommand toExecute = _command;
            if (_intervalInMs == Timeout.Infinite)
            {
                toExecute = delegate
                {
                    registry.Remove(this);
                    _command();
                };
            }
            TimerCallback timerCallBack = delegate
            {
                toExecute();
            };
            _timer = new Timer(timerCallBack, null, _firstIntervalInMs, _intervalInMs);
        }

    }

    public interface ICommandTimer
    {
        void Schedule(OnCommand command, int firstIntervalInMs);
        void ScheduleOnInterval(OnCommand command, int firstIntervalInMs, int regularIntervalInMs);
    }

    public class CommandTimer: IPendingCommandRegistry, ICommandTimer
    {
        private readonly object _lock = new object();

        private readonly ICommandQueue _queue;
        private readonly List<IPendingCommand> _pending = new List<IPendingCommand>();

        public CommandTimer(ICommandQueue queue)
        {
            _queue = queue;
        }

        public void Schedule(OnCommand comm, int timeTillEnqueueInMs)
        {
            PendingCommand pending = new PendingCommand(comm, timeTillEnqueueInMs, Timeout.Infinite);
            AddPending(pending);
        }

        public void ScheduleOnInterval(OnCommand comm, int firstInMs, int intervalInMs)
        {
            PendingCommand pending = new PendingCommand(comm, firstInMs, intervalInMs);
            AddPending(pending);
        }

        public void Remove(IPendingCommand toRemove)
        {
            lock (_lock)
            {
                _pending.Remove(toRemove);
            }
        }

        private void AddPending(PendingCommand pending)
        {
            lock (_lock)
            {
                _pending.Add(pending);
                pending.Schedule(this);
            }

        }
    }
}
