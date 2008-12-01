using System;
using System.Collections.Generic;
using System.Threading;

namespace Retlang.Core
{
    internal class ActionTimer : IPendingActionRegistry, IScheduler, IDisposable
    {
        private volatile bool _running = true;
        private readonly IDisposingExecutor _executor;
        private List<ITimerControl> _pending = new List<ITimerControl>();

        public ActionTimer(IDisposingExecutor executor)
        {
            _executor = executor;
        }

        public ITimerControl Schedule(Action action, long timeTilEnqueueInMs)
        {
            if (timeTilEnqueueInMs <= 0)
            {
                var pending = new PendingAction(action);
                _executor.Enqueue(pending.ExecuteAction);
                return pending;
            }
            else
            {
                var pending = new TimerAction(action, timeTilEnqueueInMs, Timeout.Infinite);
                AddPending(pending);
                return pending;
            }
        }

        public ITimerControl ScheduleOnInterval(Action action, long firstInMs, long regularInMs)
        {
            var pending = new TimerAction(action, firstInMs, regularInMs);
            AddPending(pending);
            return pending;
        }

        public void Remove(ITimerControl toRemove)
        {
            Action removeAction = () => _pending.Remove(toRemove);
            _executor.Enqueue(removeAction);
        }

        public void EnqueueTask(Action action)
        {
            _executor.Enqueue(action);
        }

        private void AddPending(TimerAction pending)
        {
            Action addAction = delegate
                                     {
                                         if (_running)
                                         {
                                             _pending.Add(pending);
                                             pending.Schedule(this);
                                         }
                                     };
            _executor.Enqueue(addAction);
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