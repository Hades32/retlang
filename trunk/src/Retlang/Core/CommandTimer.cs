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

        public ITimerControl Schedule(Action command, long timeTilEnqueueInMs)
        {
            if (timeTilEnqueueInMs <= 0)
            {
                var pending = new PendingCommand(command);
                _executor.Enqueue(pending.ExecuteCommand);
                return pending;
            }
            else
            {
                var pending = new TimerCommand(command, timeTilEnqueueInMs, Timeout.Infinite);
                AddPending(pending);
                return pending;
            }
        }

        public ITimerControl ScheduleOnInterval(Action command, long firstInMs, long regularInMs)
        {
            var pending = new TimerCommand(command, firstInMs, regularInMs);
            AddPending(pending);
            return pending;
        }

        public void Remove(ITimerControl toRemove)
        {
            Action removeCommand = () => _pending.Remove(toRemove);
            _executor.Enqueue(removeCommand);
        }

        public void EnqueueTask(Action toExecute)
        {
            _executor.Enqueue(toExecute);
        }

        private void AddPending(TimerCommand pending)
        {
            Action addCommand = delegate
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
            var old = Interlocked.Exchange(ref _pending, new List<ITimerControl>());
            foreach (var control in old)
            {
                control.Cancel();
            }
        }
    }
}