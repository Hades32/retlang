using System.Threading;

namespace Retlang
{
    public class PendingCommand : ITimerControl
    {
        private readonly Command _toExecute;
        private bool _cancelled;

        public PendingCommand(Command toExecute)
        {
            _toExecute = toExecute;
        }

        public void Cancel()
        {
            _cancelled = true;
        }

        public void ExecuteCommand()
        {
            if (!_cancelled)
            {
                _toExecute();
            }
        }
    }


    public class TimerCommand : ITimerControl
    {
        private readonly Command _command;
        private readonly long _firstIntervalInMs;
        private readonly long _intervalInMs;

        private Timer _timer;
        private bool _cancelled = false;

        public TimerCommand(Command command, long firstIntervalInMs, long intervalInMs)
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
            }
            if (!_cancelled)
            {
                registry.EnqueueTask(ExecuteOnProcessThread);
            }
            else
            {
                if (_timer != null)
                {
                    _timer.Dispose();
                    _timer = null;
                }
            }
        }

        public void ExecuteOnProcessThread()
        {
            if (!_cancelled)
            {
                _command();
            }
        }

        public void Cancel()
        {
            _cancelled = true;
        }
    }
}