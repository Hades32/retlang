using System;
using System.Threading;

namespace Retlang.Core
{
    internal class TimerCommand : ITimerControl
    {
        private readonly Action _command;
        private readonly long _firstIntervalInMs;
        private readonly long _intervalInMs;

        private Timer _timer;
        private bool _cancelled;

        public TimerCommand(Action command, long firstIntervalInMs, long intervalInMs)
        {
            _command = command;
            _firstIntervalInMs = firstIntervalInMs;
            _intervalInMs = intervalInMs;
        }

        public void Schedule(IPendingCommandRegistry registry)
        {
            TimerCallback timerCallBack = delegate { ExecuteOnTimerThread(registry); };
            _timer = new Timer(timerCallBack, null, _firstIntervalInMs, _intervalInMs);
        }

        public void ExecuteOnTimerThread(IPendingCommandRegistry registry)
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
                _command();
            }
        }

        public virtual void Cancel()
        {
            _cancelled = true;
        }
    }
}