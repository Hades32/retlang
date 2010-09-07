using System;
using System.Threading;

namespace Retlang.Core
{
    internal class TimerAction : IDisposable
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

        public void Schedule(ISchedulerRegistry registry)
        {
            _timer = new Timer(x => ExecuteOnTimerThread(registry), null, _firstIntervalInMs, _intervalInMs);
        }

        public void ExecuteOnTimerThread(ISchedulerRegistry registry)
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
                registry.Enqueue(ExecuteOnFiberThread);
            }
        }

        public void ExecuteOnFiberThread()
        {
            if (!_cancelled)
            {
                _action();
            }
        }

        public virtual void Dispose()
        {
            _cancelled = true;
        }

        public override string ToString()
        {
            return _action.ToString();
        }
    }
}