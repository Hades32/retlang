using System.Collections.Generic;
using System.Threading;

namespace Retlang
{
    public interface IPendingCommandRegistry
    {
        void Remove(ITimerControl timer);
        void EnqueueTask(Command command);
    }

    public interface ITimerControl
    {
        void Cancel();
    }

    public interface ICommandTimer
    {
        ITimerControl Schedule(Command command, long firstIntervalInMs);
        ITimerControl ScheduleOnInterval(Command command, long firstIntervalInMs, long regularIntervalInMs);
    }

    public class CommandTimer : IPendingCommandRegistry, ICommandTimer
    {
        private readonly ICommandQueue _queue;
        private readonly List<ITimerControl> _pending = new List<ITimerControl>();

        public CommandTimer(ICommandQueue queue)
        {
            _queue = queue;
        }

        public ITimerControl Schedule(Command comm, long timeTillEnqueueInMs)
        {
            if (timeTillEnqueueInMs <= 0)
            {
                PendingCommand pending = new PendingCommand(comm);
                _queue.Enqueue(pending.ExecuteCommand);
                return pending;
            }
            else
            {
                TimerCommand pending = new TimerCommand(comm, timeTillEnqueueInMs, Timeout.Infinite);
                AddPending(pending);
                return pending;
            }
        }

        public ITimerControl ScheduleOnInterval(Command comm, long firstInMs, long intervalInMs)
        {
            TimerCommand pending = new TimerCommand(comm, firstInMs, intervalInMs);
            AddPending(pending);
            return pending;
        }

        public void Remove(ITimerControl toRemove)
        {
            Command removeCommand = delegate { _pending.Remove(toRemove); };
            _queue.Enqueue(removeCommand);
        }

        public void EnqueueTask(Command toExecute)
        {
            _queue.Enqueue(toExecute);
        }

        private void AddPending(TimerCommand pending)
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