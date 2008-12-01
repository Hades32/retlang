using System;
using System.Threading;

namespace Retlang.Core
{
    internal class TimerAction : ITimerControl
    {
        private readonly Action _action;
        private readonly long _firstIntervalInMs;
        private readonly long _intervalInMs;

        private Timer _timer;
        private bool _cancelled;

        public TimerAction(Action action, long firstIntervalInMs, long intervalInMs)
        {
            _action = action;
            _firstIntervalInMs = firstIntervalInMs;
            _intervalInMs = intervalInMs;
        }

        public void Schedule(IPendingActionRegistry registry)
        {
            TimerCallback timerCallBack = delegate { ExecuteOnTimerThread(registry); };
            _timer = new Timer(timerCallBack, null, _firstIntervalInMs, _intervalInMs);
        }

        public void ExecuteOnTimerThread(IPendingActionRegistry registry)
        {
            if (_intervalInMs == Timeout.Infinite || _cancelled)
            {
                registry.Remove(this);
                if (_timer != null)
                {
                    _timer.Dispose();
                    _timer = null;
                }
            }

            if (!_cancelled)
            {
                registry.EnqueueTask(ExecuteOnProcessThread);
            }
        }

        public void ExecuteOnProcessThread()
        {
            if (!_cancelled)
            {
                _action();
            }
        }

        public virtual void Cancel()
        {
            _cancelled = true;
        }
    }
}