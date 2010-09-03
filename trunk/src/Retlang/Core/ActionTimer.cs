using System;
using System.Collections.Generic;
using System.Threading;

namespace Retlang.Core
{
    ///<summary>
    /// Enqueues actions on to executor after schedule elapses.  
    ///</summary>
    public class ActionTimer : IPendingActionRegistry, IScheduler, IDisposable
    {
        private volatile bool _running = true;
        private readonly IDisposingExecutor _executor;
        private List<ITimerControl> _pending = new List<ITimerControl>();

        ///<summary>
        /// Constructs new instance.
        ///</summary>
        public ActionTimer(IDisposingExecutor executor)
        {
            _executor = executor;
        }

        ///<summary>
        /// Enqueues action on to executor after timer elapses.  
        ///</summary>
        public ITimerControl Schedule(Action action, long firstInMs)
        {
            if (firstInMs <= 0)
            {
                var pending = new PendingAction(action);
                _executor.Enqueue(pending.ExecuteAction);
                return pending;
            }
            else
            {
                var pending = new TimerAction(action, firstInMs, Timeout.Infinite);
                AddPending(pending);
                return pending;
            }
        }

        ///<summary>
        /// Enqueues actions on to executor after schedule elapses.  
        ///</summary>
        public ITimerControl ScheduleOnInterval(Action action, long firstInMs, long regularInMs)
        {
            var pending = new TimerAction(action, firstInMs, regularInMs);
            AddPending(pending);
            return pending;
        }

        ///<summary>
        /// Removes a pending scheduled action.
        ///</summary>
        ///<param name="toRemove"></param>
        public void Remove(ITimerControl toRemove)
        {
            Action removeAction = () => _pending.Remove(toRemove);
            _executor.Enqueue(removeAction);
        }

        ///<summary>
        /// Enqueues actions on to executor immediately.
        ///</summary>
        ///<param name="action"></param>
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

        ///<summary>
        /// Cancels all pending actions
        ///</summary>
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