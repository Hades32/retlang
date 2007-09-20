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

    public class PendingCommand : IPendingCommand
    {
        private readonly Command _command;
        private readonly int _firstIntervalInMs;
        private readonly int _intervalInMs;

        private Timer _timer;

        public PendingCommand(Command command, int firstIntervalInMs, int intervalInMs)
        {
            _command = command;
            _firstIntervalInMs = firstIntervalInMs;
            _intervalInMs = intervalInMs;
        }

        public void Schedule(IPendingCommandRegistry registry)
        {
            Command toExecute = _command;
            if (_intervalInMs == Timeout.Infinite)
            {
                toExecute = delegate
                                {
                                    registry.Remove(this);
                                    _command();
                                };
            }
            TimerCallback timerCallBack = delegate { toExecute(); };
            _timer = new Timer(timerCallBack, null, _firstIntervalInMs, _intervalInMs);
        }
    }

    public interface ICommandTimer
    {
        void Schedule(Command command, int firstIntervalInMs);
        void ScheduleOnInterval(Command command, int firstIntervalInMs, int regularIntervalInMs);
    }

    public class CommandTimer : IPendingCommandRegistry, ICommandTimer
    {
        private readonly ICommandQueue _queue;
        private readonly List<IPendingCommand> _pending = new List<IPendingCommand>();

        public CommandTimer(ICommandQueue queue)
        {
            _queue = queue;
        }

        public void Schedule(Command comm, int timeTillEnqueueInMs)
        {
            PendingCommand pending = new PendingCommand(comm, timeTillEnqueueInMs, Timeout.Infinite);
            AddPending(pending);
        }

        public void ScheduleOnInterval(Command comm, int firstInMs, int intervalInMs)
        {
            PendingCommand pending = new PendingCommand(comm, firstInMs, intervalInMs);
            AddPending(pending);
        }

        public void Remove(IPendingCommand toRemove)
        {
            Command removeCommand = delegate { _pending.Remove(toRemove); };
            _queue.Enqueue(removeCommand);
        }

        private void AddPending(PendingCommand pending)
        {
            Command addCommand = delegate
                                     {
                                         _pending.Add(pending);
                                         pending.Schedule(this);
                                     };
            _queue.Enqueue(addCommand);
        }
    }
}