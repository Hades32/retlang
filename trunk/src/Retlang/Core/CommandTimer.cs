using System;
using System.Collections.Generic;
using System.Threading;

namespace Retlang.Core
{
    internal class CommandTimer : IPendingCommandRegistry, IScheduler, IDisposable
    {
        private volatile bool _running = true;
        private readonly IDisposingExecutor _executor;
        private List<ITimerControl> _pending = new List<ITimerControl>();

        public CommandTimer(IDisposingExecutor executor)
        {
            _executor = executor;
        }

        public ITimerControl Schedule(Command command, long timeTilEnqueueInMs)
        {
            if (timeTilEnqueueInMs <= 0)
            {
                PendingCommand pending = new PendingCommand(command);
                _executor.Enqueue(pending.ExecuteCommand);
                return pending;
            }
            else
            {
                TimerCommand pending = new TimerCommand(command, timeTilEnqueueInMs, Timeout.Infinite);
                AddPending(pending);
                return pending;
            }
        }

        public ITimerControl ScheduleOnInterval(Command command, long firstInMs, long regularInMs)
        {
            TimerCommand pending = new TimerCommand(command, firstInMs, regularInMs);
            AddPending(pending);
            return pending;
        }

        public void Remove(ITimerControl toRemove)
        {
            Command removeCommand = delegate { _pending.Remove(toRemove); };
            _executor.Enqueue(removeCommand);
        }

        public void EnqueueTask(Command toExecute)
        {
            _executor.Enqueue(toExecute);
        }

        private void AddPending(TimerCommand pending)
        {
            Command addCommand = delegate
                                     {
                                         if (_running)
                                         {
                                             _pending.Add(pending);
                                             pending.Schedule(this);
                                         }
                                     };
            _executor.Enqueue(addCommand);
        }

        public void Dispose()
        {
            _running = false;
            List<ITimerControl> old = Interlocked.Exchange(ref _pending, new List<ITimerControl>());
            foreach (ITimerControl control in old)
            {
                control.Cancel();
            }
        }
    }
}